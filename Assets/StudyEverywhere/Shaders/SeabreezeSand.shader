// Seabreeze's beach: a photographed wavy-sand texture (albedo, normal, AO), warmed to coral sand and sampled at two
// scales so it never visibly tiles, laid in world XZ. Where the sea has just washed (below _SeaWet, set by SeaShore) the
// sand is dark and glossy; under the water it takes the lagoon's colour with depth and drifting caustics. In the writing
// patch (_WriteRect: x, z of its corner and its size in metres) a finger's trace in _WriteMap (R: how deep) is pressed in
// as a groove with soft raised lips. Lit by the sun (with shadows), the sky and the fire bowl.
Shader "StudyEverywhere/Seabreeze Sand"
{
    Properties
    {
        _BaseMap ("Sand albedo (grey)", 2D) = "grey" {}
        [Normal] _BumpMap ("Sand normal", 2D) = "bump" {}
        _OcclusionMap ("Sand AO", 2D) = "white" {}
        _Tint ("Dry sand tint", Color) = (1.55, 1.45, 1.27, 1)
        _Tile ("Metres per tile", Float) = 2.2
        _BumpScale ("Ripple strength", Range(0, 2)) = 0.8
        _Noise ("Noise (R, G broad; B grain)", 2D) = "grey" {}
        _GrooveDepth ("Groove relief", Float) = 1.6
        _Absorb ("Water absorption per metre (rgb)", Vector) = (2.6, 0.75, 0.5, 0)
        _Lagoon ("Lagoon colour", Color) = (0.05, 0.42, 0.45, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_OcclusionMap); SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_Noise); SAMPLER(sampler_Noise);
            TEXTURE2D(_WriteMap); SAMPLER(sampler_WriteMap);
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint, _Lagoon;
                float _Tile, _BumpScale, _GrooveDepth;
                float4 _Absorb;
            CBUFFER_END
            float _SeaLevel, _SeaWet;   // SeaShore
            float4 _WriteRect;          // SandWriting: the patch (x, z of its corner, size, one texel in uv); _WriteMap holds its height in cm

            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; half4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float3 p : TEXCOORD0; float3 n : TEXCOORD1; half fog : TEXCOORD2; half4 color : COLOR; UNITY_VERTEX_OUTPUT_STEREO };

            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i); ZERO_INITIALIZE(V, o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.p = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.p);
                o.n = TransformObjectToWorldNormal(i.normalOS);
                o.fog = ComputeFogFactor(o.positionCS.z);
                o.color = i.color;
                return o;
            }

            float Mark(float2 uv) { return SAMPLE_TEXTURE2D_LOD(_WriteMap, sampler_WriteMap, uv, 0).r; }

            half4 frag(V i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 w = i.p.xz;
                // the photo's ripples turned a quarter round (u along z, v along x), so they lie along the shore, as
                // the waves coming in up the beach would leave them; the normal map's slope is turned back below
                float2 turned = w.yx;
                float2 uv1 = turned / _Tile, uv2 = turned / (_Tile * 3.7) + float2(0.37, 0.71);
                half4 broad = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, w * 0.021);
                half blend = smoothstep(0.35, 0.65, broad.r);
                half3 a1 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv1).rgb, a2 = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv2).rgb;
                half3 albedo = lerp(a1, a2, blend * 0.6) * _Tint.rgb * (0.9 + 0.2 * broad.g) * i.color.rgb;   // vertex colour: darker leaf-littered ground under the palms
                half ao = lerp(SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv1).r, 1, 0.3);
                half3 t1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv1), _BumpScale);
                half3 t2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv2), _BumpScale);
                half2 slope = lerp(t1.yx, t2.yx, blend * 0.6);   // back to world x, z

                // The writing: grooves (negative) and their lips (positive), in cm; the relief tilts the normal.
                float2 wuv = (w - _WriteRect.xy) / max(_WriteRect.z, 1e-3);
                if (_WriteRect.z > 0 && all(wuv > 0) && all(wuv < 1))
                {
                    float e = _WriteRect.w;   // one texel
                    float h = Mark(wuv), hx = Mark(wuv + float2(e, 0)) - Mark(wuv - float2(e, 0)), hz = Mark(wuv + float2(0, e)) - Mark(wuv - float2(0, e));
                    slope -= float2(hx, hz) * _GrooveDepth;
                    ao *= 1 - saturate(-h) * 0.4;
                    albedo *= 1 - saturate(-h) * 0.1;
                }
                float3 n = normalize(normalize(i.n) + float3(slope.x, 0, slope.y));

                // Wet behind the last wave, and always just above the water; glossy and darker.
                float wet = saturate((max(_SeaWet, _SeaLevel) + 0.025 - i.p.y) / 0.04);
                albedo *= lerp(1, 0.58, wet);
                float under = _SeaLevel - i.p.y;

                Light sun = GetMainLight(TransformWorldToShadowCoord(i.p));
                float3 v = normalize(GetWorldSpaceViewDir(i.p));
                half ndl = saturate(dot(n, sun.direction));
                half3 light = sun.color * sun.shadowAttenuation * ndl + SampleSH(n) * ao;
            #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint k = 0; k < count; k++) { Light l = GetAdditionalLight(k, i.p); light += l.color * l.distanceAttenuation * saturate(dot(n, l.direction)); }
            #endif
                half3 c = albedo * light;
                // Wet sand mirrors the sky a little and catches the sun.
                half3 r = reflect(-v, n);
                half fres = 0.04 + 0.96 * pow(1 - saturate(dot(n, v)), 5);
                c += wet * (GlossyEnvironmentReflection(r, 0.25, 1) * fres * 0.6 + sun.color * sun.shadowAttenuation * pow(saturate(dot(r, sun.direction)), 120) * 0.8);

                if (under > 0)
                {
                    // Under the lagoon: caustics dancing on the bottom, then the water's colour with depth (the
                    // path in the water grows as the view flattens).
                    float t = _Time.y;
                    float2 q = w * 0.55;
                    half n1 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, q + float2(0.031, 0.017) * t).g;
                    half n2 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, q * 1.3 + float2(-0.023, 0.029) * t + 0.5).g;
                    half caustic = pow(saturate(1 - abs(n1 - n2) * 7), 6);
                    c += sun.color * sun.shadowAttenuation * caustic * 0.9 * saturate(under * 6) * exp(-under * 0.6) * albedo;
                    float path = under * (1 + 1 / max(v.y, 0.2));
                    half3 trans = exp(-_Absorb.rgb * path);
                    half3 water = lerp(_Lagoon.rgb, _Lagoon.rgb * float3(0.25, 0.4, 0.75), saturate(under / 3)) * (SampleSH(float3(0, 1, 0)) + sun.color * saturate(sun.direction.y) * 0.5);   // deeper, bluer
                    c = c * trans + water * (1 - trans);
                }
                return half4(MixFog(c, i.fog), 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}

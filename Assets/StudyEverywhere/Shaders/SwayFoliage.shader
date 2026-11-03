// Leaves and slender stems that move in the air, for merged foliage (the Seoul courtyard's pine pads, maple clumps,
// bamboo, ferns): lit like URP Lit (the sun with its shadows, the ambient sky, the lamps), but each vertex sways with
// the breeze in world space, more the higher it stands above _SwayBase (nothing at the foot, the full _SwayAmp from
// _SwayHeight up), a slow lean that comes and goes in gusts plus a quicker shiver of the leaves, phased by position so
// neighbouring clumps never move in step. One merged mesh, no script: the motion is all in the vertex shader.
Shader "StudyEverywhere/Sway Foliage"
{
    Properties
    {
        _BaseMap ("Colour", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.3
        _SwayBase ("Sway starts at height (m, world)", Float) = 0
        _SwayHeight ("Full sway this far above it (m)", Float) = 3
        _SwayAmp ("Sway (m at full height)", Float) = 0.04
        _Shiver ("Leaf shiver (m)", Float) = 0.004
        _WindDir ("Wind direction (xz)", Vector) = (0.8, 0, 0.6, 0)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull [_Cull]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; half4 _BaseColor; half _Smoothness; float _SwayBase, _SwayHeight, _SwayAmp, _Shiver; float4 _WindDir; half _Cull;
        CBUFFER_END
        float3 Sway(float3 w)
        {
            float t = _Time.y, k = saturate((w.y - _SwayBase) / max(_SwayHeight, 0.01));
            k *= k;   // stiff low down, freer toward the tips
            float phase = dot(w.xz, float2(0.37, 0.61));
            float gust = 0.55 + 0.45 * sin(t * 0.21 + phase * 0.3) * sin(t * 0.13 + 1.7);
            float lean = (sin(t * 0.9 + phase) * 0.6 + sin(t * 1.7 + phase * 1.9) * 0.25 + 0.35) * gust;
            float3 dir = normalize(float3(_WindDir.x, 0, _WindDir.z) + 1e-4);
            float3 d = dir * lean * _SwayAmp * k;
            d += float3(sin(t * 6.1 + w.x * 9.3), sin(t * 7.3 + w.y * 8.1) * 0.5, sin(t * 5.7 + w.z * 9.7)) * _Shiver * saturate(k * 3) * gust;
            return w + d;
        }
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionWS : TEXCOORD1; half3 normalWS : TEXCOORD2; half fog : TEXCOORD3; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                ZERO_INITIALIZE(Varyings, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionWS = Sway(TransformObjectToWorld(v.positionOS.xyz));
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap); o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb * _BaseColor.rgb;
                float3 w = i.positionWS; half3 view = GetWorldSpaceNormalizeViewDir(w);
                half3 n = normalize(i.normalWS); if (_Cull < 0.5 && dot(n, view) < 0) n = -n;   // a two-sided leaf faces the eye
                InputData input = (InputData)0;
                input.positionWS = w; input.normalWS = n; input.viewDirectionWS = view;
                input.shadowCoord = TransformWorldToShadowCoord(w); input.fogCoord = i.fog; input.bakedGI = SampleSH(n);
                input.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS); input.shadowMask = 1;
                SurfaceData s = (SurfaceData)0;
                s.albedo = albedo; s.smoothness = _Smoothness; s.occlusion = 1; s.alpha = 1;
                half4 c = UniversalFragmentPBR(input, s);
                return half4(MixFog(c.rgb, i.fog), 1);
            }
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; UNITY_VERTEX_OUTPUT_STEREO };
            Varyings vert(Attributes v)
            {
                Varyings o; UNITY_SETUP_INSTANCE_ID(v); ZERO_INITIALIZE(Varyings, o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformWorldToHClip(Sway(TransformObjectToWorld(v.positionOS.xyz))); return o;
            }
            half4 frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}

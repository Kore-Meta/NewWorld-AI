// Seabreeze's palms and beach shrubs in the sea breeze: the whole plant leans and sways a little more the higher it goes,
// and the leaves flutter more toward their tips. The plants are merged into a few meshes, so each vertex carries its
// plant's data in uv1: x how far up the plant it is (0 at the foot, 1 at the top), y its distance out from the trunk
// (m), z the plant's own phase. Albedo and normal from the plant's textures, cut out by alpha (_Cutoff) and lit from
// both sides, with the sun glowing through the leaves; the shadow pass moves with it.
Shader "StudyEverywhere/Seabreeze Palm"
{
    Properties
    {
        _BaseMap ("Albedo (A cut-out)", 2D) = "white" {}
        [Normal] _BumpMap ("Normal", 2D) = "bump" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Cutoff ("Alpha cut-off", Range(0, 1)) = 0.45
        _Sway ("Sway at the top (m)", Float) = 0.18
        _Flutter ("Leaf flutter (m)", Float) = 0.06
        _Translucency ("Light through leaves", Range(0, 2)) = 0.6
        _WindDir ("Wind direction (xz)", Vector) = (-0.4, 0, -0.9, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" }
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST, _WindDir;
            half4 _Color;
            half _Cutoff, _Translucency;
            float _Sway, _Flutter;
        CBUFFER_END

        float3 Breeze(float3 wp, float3 plant)
        {
            float h = plant.x, out_ = plant.y;
            float t = _Time.y, ph = plant.z * 6.2832;
            float2 wind = normalize(_WindDir.xz);
            float sway = (0.55 + 0.45 * sin(t * 0.7 + ph)) * (0.8 + 0.2 * sin(t * 1.9 + ph * 2.1));
            float3 d = float3(wind.x, 0, wind.y) * sway * _Sway * h * h;
            // the leaves: a quick flutter growing toward the frond tips
            float f = sin(t * 3.1 + dot(wp, float3(1.7, 2.3, 1.3))) * 0.6 + sin(t * 5.3 + dot(wp, float3(-2.1, 1.1, 2.9))) * 0.4;
            d += float3(wind.x * 0.4, 1, wind.y * 0.4) * f * _Flutter * saturate(out_ / 3) * h;
            return wp + d;
        }
        ENDHLSL

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

            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 tangentOS : TANGENT; float2 uv : TEXCOORD0; float3 plant : TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 p : TEXCOORD1; float3 n : TEXCOORD2; float4 t : TEXCOORD3; half fog : TEXCOORD4; UNITY_VERTEX_OUTPUT_STEREO };

            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i); ZERO_INITIALIZE(V, o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.p = Breeze(TransformObjectToWorld(i.positionOS.xyz), i.plant);
                o.positionCS = TransformWorldToHClip(o.p);
                o.n = TransformObjectToWorldNormal(i.normalOS);
                o.t = float4(TransformObjectToWorldDir(i.tangentOS.xyz), i.tangentOS.w * GetOddNegativeScale());
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(V i, bool front : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _Color;
                clip(a.a - _Cutoff);
                float3 n = normalize(i.n), t = normalize(i.t.xyz), b = cross(n, t) * i.t.w;
                half3 tn = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv));
                n = normalize(t * tn.x + b * tn.y + n * tn.z);
                if (!front) n = -n;
                Light sun = GetMainLight(TransformWorldToShadowCoord(i.p));
                half shade = sun.shadowAttenuation;
                half3 light = sun.color * shade * saturate(dot(n, sun.direction) * 0.75 + 0.25) + SampleSH(n);
                // the sun through the leaves, seen from their shaded side
                light += sun.color * shade * _Translucency * saturate(dot(-n, sun.direction)) * a.g;
            #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint k = 0; k < count; k++) { Light l = GetAdditionalLight(k, i.p); light += l.color * l.distanceAttenuation * saturate(dot(n, l.direction) * 0.7 + 0.3); }
            #endif
                return half4(MixFog(a.rgb * light, i.fog), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            float3 _LightDirection;
            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; float3 plant : TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i);
                float3 p = Breeze(TransformObjectToWorld(i.positionOS.xyz), i.plant), n = TransformObjectToWorldNormal(i.normalOS);
                o.positionCS = TransformWorldToHClip(ApplyShadowBias(p, n, _LightDirection));
            #if UNITY_REVERSED_Z
                o.positionCS.z = min(o.positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                o.positionCS.z = max(o.positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                return o;
            }
            half4 frag(V i) : SV_Target { clip(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).a * _Color.a - _Cutoff); return 0; }
            ENDHLSL
        }
    }
}

// The Seoul café's courtyard pond: a still, dark garden pond that is never quite still. Two layers of slow ripples
// drift across it in different directions, with a soft breeze that gathers and fades over the surface in patches, so
// the sky in it wavers; it reflects the sky's own light (the ambient sky the time of day sets, so dawn, noon, dusk and
// night all show in it), glints where the sun or the moon catches the ripples, and picks up the garden lights' warm
// points at night. Opaque and one pass (a small pond on the Quest: no reflection probe, no grab pass).
Shader "StudyEverywhere/Pond Water"
{
    Properties
    {
        [NoScaleOffset] _Ripples ("Ripple noise (RG slopes)", 2D) = "grey" {}
        _RippleScale ("Ripple tiles a metre", Float) = 0.55
        _RippleStrength ("Ripple strength", Range(0, 1)) = 0.35
        _Drift ("Drift (m/s)", Float) = 0.05
        _Deep ("Water", Color) = (0.03, 0.05, 0.045, 1)
        _SkyTint ("Reflected sky", Color) = (0.9, 0.95, 1, 1)
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
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _RippleScale, _Drift; half _RippleStrength; half4 _Deep, _SkyTint;
            CBUFFER_END
            TEXTURE2D(_Ripples); SAMPLER(sampler_Ripples);

            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                ZERO_INITIALIZE(Varyings, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float3 view = normalize(i.positionWS - _WorldSpaceCameraPos);
                float t = _Time.y;
                float2 p = i.positionWS.xz * _RippleScale;
                // a breeze: a slow large-scale swell that raises the ripples in passing patches
                half breeze = SAMPLE_TEXTURE2D(_Ripples, sampler_Ripples, p * 0.18 + float2(0.02, -0.013) * t).r;
                half gust = 0.45 + 0.9 * smoothstep(0.4, 0.62, breeze);
                // two fine ripple layers crossing in near-opposite directions (one layer sliding alone read as a film over
                // the water: the user, Oct 26 morning), the second smaller and quicker
                half2 a = SAMPLE_TEXTURE2D(_Ripples, sampler_Ripples, p + float2(0.6, 0.35) * _Drift * t).rg - 0.5;
                half2 b = SAMPLE_TEXTURE2D(_Ripples, sampler_Ripples, p * 2.3 + float2(-0.55, -0.6) * _Drift * 1.9 * t).rg - 0.5;
                half2 slope = (a + b * 0.6) * _RippleStrength * gust;
                // and wavelets that travel: four trains of small waves, each its own length, heading and speed (a calm
                // pond's, half a free wave's), so the surface itself moves and the lights' reflections break and dance
                float2 w = i.positionWS.xz;
                const float2 dir[4] = { float2(0.96, 0.28), float2(-0.37, 0.93), float2(0.75, -0.66), float2(-0.98, -0.2) };
                const float len[4] = { 0.85, 0.52, 0.31, 0.19 };
                [unroll] for (int k = 0; k < 4; k++)
                {
                    float kw = 6.2832 / len[k], speed = 0.5 * sqrt(9.81 / kw);
                    slope += dir[k] * (0.055 * cos(kw * (dot(dir[k], w) - speed * t) + k * 1.7)) * gust;
                }
                float3 n = normalize(float3(slope.x, 1, slope.y));
                float3 r = reflect(view, n); r.y = max(r.y, 0.02);
                half F = 0.03 + 0.97 * pow(1 - saturate(dot(-view, n)), 5);
                // the sky it mirrors: the ambient sky in that direction (the clock sets it), brightened toward the zenith
                half3 sky = max(SampleSH(r), 0) * _SkyTint.rgb * (0.8 + 0.5 * r.y);
                half3 body = _Deep.rgb * (0.6 + 0.4 * max(SampleSH(float3(0, 1, 0)), 0));
                half3 c = lerp(body, sky, saturate(F + 0.22));
                // the sun (or the moon) glinting on the ripples
                Light sun = GetMainLight();
                half glint = pow(saturate(dot(r, sun.direction)), 600) * 6 + pow(saturate(dot(r, sun.direction)), 60) * 0.15;
                c += sun.color * glint;
                #if defined(_ADDITIONAL_LIGHTS)
                // the garden's hidden lights and the lanterns: small warm points dancing on it at night
                uint count = GetAdditionalLightsCount();
                for (uint k = 0; k < count; k++)
                {
                    Light l = GetAdditionalLight(k, i.positionWS);
                    c += l.color * l.distanceAttenuation * pow(saturate(dot(r, l.direction)), 220) * 2.5;
                }
                #endif
                return half4(min(c, 0.98), 1);   // under 1: no HDR on the Quest, past it a glint clips to a flat white patch
            }
            ENDHLSL
        }
    }
}

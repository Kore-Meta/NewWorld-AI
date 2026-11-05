// Seabreeze's lagoon: one transparent sheet from the shore to the horizon. The whole sheet rises and falls with the
// swash (_SeaLevel, set by SeaShore), so its edge runs up the beach and slides back; a low swell rolls over it where it
// is deep enough. Each vertex carries the height of the sand under it (uv1.x), so the depth is known everywhere: the
// clear shallows show the sand (the Seabreeze Sand shader tints it with depth and lays caustics on it), the water only
// adds the sky's reflection (Fresnel), the sun's glitter, a lace of foam at the very edge, and, out where the sand
// ends, its own deep blue. Fingers dipped in the water send out rings (_SeaRipples: x, z, start time, strength).
Shader "StudyEverywhere/Seabreeze Water"
{
    Properties
    {
        _Noise ("Noise (R, G slopes; B lace)", 2D) = "grey" {}
        _Deep ("Deep water", Color) = (0.01, 0.12, 0.24, 1)
        _Shallow ("Shallow water", Color) = (0.06, 0.45, 0.48, 1)
        _RippleScale ("Ripple scale", Float) = 0.45
        _RippleStrength ("Ripple strength", Range(0, 1)) = 0.22
        _Swell ("Swell height", Float) = 0.035
        _Foam ("Foam colour", Color) = (0.95, 0.97, 0.96, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-10" "RenderPipeline" = "UniversalPipeline" }
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_Noise); SAMPLER(sampler_Noise);
            CBUFFER_START(UnityPerMaterial)
                half4 _Deep, _Shallow, _Foam;
                float _RippleScale, _RippleStrength, _Swell;
            CBUFFER_END
            float _SeaLevel;
            float _SeaCalm;   // 0..1: Calypso calming the sea (SeaShore): the swell flattens and the ripples settle to a faint stir
            float4 _SeaRipples[4];

            struct A { float4 positionOS : POSITION; float2 uv1 : TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float3 p : TEXCOORD0; float sand : TEXCOORD1; half fog : TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };

            float SwellAt(float2 xz, float depth)
            {
                float t = _Time.y;
                float s = sin(dot(xz, float2(0.08, 0.55)) - t * 0.9) * 0.6 + sin(dot(xz, float2(-0.21, 0.37)) - t * 1.3) * 0.3 + sin(dot(xz, float2(0.5, 0.2)) - t * 1.7) * 0.1;
                return s * _Swell * saturate(depth * 0.8) * (1 - _SeaCalm);
            }

            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i); ZERO_INITIALIZE(V, o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 p = TransformObjectToWorld(i.positionOS.xyz);
                float depth = _SeaLevel - i.uv1.x;
                p.y = _SeaLevel + SwellAt(p.xz, depth);
                o.p = p; o.sand = i.uv1.x;
                o.positionCS = TransformWorldToHClip(p);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(V i) : SV_Target   // seen from above only: the far skirt is drawn whichever way it winds
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float t = _Time.y;
                float depth = max(0, i.p.y - i.sand);
                float3 toEye = GetWorldSpaceViewDir(i.p); float dist = length(toEye); float3 v = toEye / dist;

                // Ripples: two drifting layers of noise slopes, calmer far off (a smooth mirror toward the horizon).
                float2 q = i.p.xz * _RippleScale;
                half2 s1 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, q * 0.31 + float2(0.013, 0.021) * t).rg - 0.5;
                half2 s2 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, q * 0.83 + float2(-0.019, 0.011) * t).rg - 0.5;
                half2 s3 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, q * 2.3 + float2(0.027, -0.017) * t).rg - 0.5;
                half fade = 1 / (1 + dist * 0.03);
                half2 slope = (s1 + s2 * 0.7 + s3 * 0.45 * fade) * _RippleStrength * (0.35 + 0.65 * fade) * (1 - 0.8 * _SeaCalm);
                // Rings from a hand in the water.
                for (int k = 0; k < 4; k++)
                {
                    float4 rp = _SeaRipples[k]; float age = t - rp.z;
                    if (rp.w <= 0 || age < 0 || age > 3) continue;
                    float2 d = i.p.xz - rp.xy; float r = length(d) + 1e-4;
                    float wave = r - age * 0.32;
                    float ring = sin(wave * 38) * exp(-wave * wave * 90) * exp(-age * 1.4) * rp.w;
                    slope += d / r * ring * 0.9;
                }
                float3 n = normalize(float3(slope.x, 1, slope.y));

                Light sun = GetMainLight(TransformWorldToShadowCoord(i.p));
                float3 r = reflect(-v, n); r.y = abs(r.y);
                half F = 0.02 + 0.98 * pow(1 - saturate(dot(n, v)), 5);
                half3 refl = GlossyEnvironmentReflection(r, 0.02, 1);
                // The sun's glitter: a tight glint on the ripple crests and a softer sheen round it.
                half glint = pow(saturate(dot(r, sun.direction)), 900) * 40 + pow(saturate(dot(r, sun.direction)), 90) * 0.6;
                half3 spec = sun.color * glint * lerp(0.4, 1, sun.shadowAttenuation);

                // The water's own colour shows only where it is deep (past the end of the sand) or seen very flat.
                half body = smoothstep(.9, 3.2, depth);   // opaque by the time the sand under it ends
                half3 lit = SampleSH(float3(0, 1, 0)) + sun.color * saturate(sun.direction.y) * 0.4;
                half3 bodyCol = lerp(_Shallow.rgb, _Deep.rgb, smoothstep(1.5, 6, depth)) * lit;

                // Foam: a fine lace right at the edge, breaking up as it thins.
                half lace = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, i.p.xz * 1.7 + float2(0.05, 0.03) * t).b;
                half lace2 = SAMPLE_TEXTURE2D(_Noise, sampler_Noise, i.p.xz * 0.9 - float2(0.02, 0.04) * t).g;
                half edge = 1 - smoothstep(0.0, 0.035, depth);
                half foam = saturate(edge * smoothstep(0.35, 0.75, lace * 0.7 + lace2 * 0.6)) * 0.85;
                // A soft edge where the sheet meets the sand, so the line is never hard.
                half feather = smoothstep(0.0, 0.006, depth);

                half a = saturate(F + body * (1 - F)) * feather;
                half3 c = refl * F + bodyCol * body * (1 - F) + spec * feather;
                half3 foamCol = _Foam.rgb * lit;
                c = c * (1 - foam) + foamCol * foam;
                a = saturate(a * (1 - foam) + foam);
                c = MixFog(c, i.fog);
                return half4(c, a);
            }
            ENDHLSL
        }
    }
}

// Moving water for the lobby's rock waterfall, procedural (no textures). _Pool 0: a falling sheet on a quad, uv.y down
// the fall: streaks of white water pulled down at _Speed, breaking up as they fall, thinner and clearer at the sides,
// a frothy lip at the top and churned foam at the foot. _Pool 1: the basin's surface: soft ripples spreading out from
// where the water lands (uv (0.5, 1) is the fall's foot), glints of the sky, foam near the fall. See-through, unlit.
Shader "StudyEverywhere/Waterfall"
{
    Properties
    {
        _Deep ("Water", Color) = (0.42, 0.58, 0.62, 0.55)
        _Foam ("White water", Color) = (0.94, 0.97, 1, 1)
        _Speed ("Fall speed", Float) = 0.55
        _Pool ("Pool surface", Float) = 0
        _Aspect ("Width over height (uv)", Float) = 0.4
        _Glint ("Sky glints on the pool", Float) = 1
        _Wrap ("Round water (uv.x once round, no side fade)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha   // alpha stays opaque, so passthrough never shows through
        ZWrite Off
        Cull Off
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Deep, _Foam; float _Speed, _Pool, _Aspect, _Glint, _Wrap;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 posWS : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes v)
            {
                Varyings o; UNITY_SETUP_INSTANCE_ID(v); ZERO_INITIALIZE(Varyings, o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.posWS = TransformObjectToWorld(v.positionOS.xyz); o.positionCS = TransformWorldToHClip(o.posWS); o.uv = v.uv; return o;
            }

            float hash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }
            float fbm(float2 p) { float s = 0, a = .5; for (int k = 0; k < 4; k++) { s += a * noise(p); p = p * 2.03 + 7.1; a *= .5; } return s; }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float t = _Time.y;
                if (_Pool < .5)
                {
                    float2 uv = i.uv;
                    // streaks: stretched noise pulled down the fall, faster lower down as the water speeds up
                    float fall = uv.y * (1 + uv.y * .6);
                    float streak = fbm(float2(uv.x * 26 * _Aspect * 2.5, fall * 7 - t * _Speed * 2));
                    float fine = noise(float2(uv.x * 90 * _Aspect, fall * 16 - t * _Speed * 3));
                    float churn = noise(float2(uv.x * 30, t * 3));
                    float sides = smoothstep(0, .12, uv.x) * smoothstep(1, .88, uv.x);
                    if (_Wrap > .5)
                    {
                        // round water (a jet's column and falling crown): no edges to thin out, and the noise blended with
                        // itself one turn back so the streaks meet without a seam where uv.x comes round from 1 to 0
                        float x1 = uv.x - 1, w = uv.x;
                        streak = lerp(streak, fbm(float2(x1 * 26 * _Aspect * 2.5, fall * 7 - t * _Speed * 2)), w);
                        fine = lerp(fine, noise(float2(x1 * 90 * _Aspect, fall * 16 - t * _Speed * 3)), w);
                        churn = lerp(churn, noise(float2(x1 * 30, t * 3)), w);
                        sides = 1;
                    }
                    // round water is a thin sheet seen through itself: sparser threads of white over clearer water
                    float white = _Wrap > .5 ? saturate((streak * .75 + fine * .4 - .52) * 2.6) : saturate((streak * .75 + fine * .4 - .38) * 2.2);
                    float lip = smoothstep(.06, 0, uv.y), foot = smoothstep(.82, 1, uv.y) * (.6 + .4 * churn);
                    white = saturate(white + lip * .6 + foot * (_Wrap > .5 ? .7 : 1));
                    half3 c = lerp(_Deep.rgb, _Foam.rgb, white);
                    half a = saturate(lerp(_Deep.a * (_Wrap > .5 ? .35 : .7), .95, white)) * sides;
                    return half4(c, a);
                }
                else
                {
                    // ripples spreading from the fall's foot, glints, foam where the water lands
                    float2 p = (i.uv - float2(.5, 1)) * float2(1 / max(_Aspect, .05), 1);
                    float r = length(p);
                    float ripple = sin(r * 38 - t * 3.2 + fbm(i.uv * 6 + t * .2) * 4) * .5 + .5;
                    float glint = pow(saturate(noise(i.uv * 60 + t * .7) * 1.15), 12);
                    float foam = smoothstep(.45, 0, r) * (.5 + .5 * fbm(i.uv * 20 - t * .8));
                    half3 c = _Deep.rgb * (.85 + .25 * ripple) + glint * .6 * _Glint;
                    c = lerp(c, _Foam.rgb, saturate(foam));
                    return half4(c, saturate(_Deep.a + foam * .4 + glint * .3 * _Glint));
                }
            }
            ENDHLSL
        }
    }
}

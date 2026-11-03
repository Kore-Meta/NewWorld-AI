// Calypso's orb: a soap-bubble sphere with water moving inside it. The shell is thin film: clear where seen straight on,
// a blue-to-cream iridescent rim toward the edge, the sun's highlight and a soft window highlight up and to the left. The
// inside is swirling water, sampled at three depths along each eye ray through the ball (an analytic chord, no textures),
// so the blue streaks and milky foam lie in depth and drift round. CalypsoOrb drives it: _Flow is the waves' phase
// (it runs faster as she thinks or talks), _Speak swells the waves and sends rings out from the centre and quivers the
// shell as she talks, _Listen sends rings in from the side facing the person as they speak to her. Mostly self-lit, so
// it reads at night. A smooth sphere of radius 0.5 (SeabreezeCalypso builds one).
Shader "StudyEverywhere/Calypso Orb"
{
    Properties
    {
        _Deep ("Deep water", Color) = (0.22, 0.36, 0.93, 1)
        _Shallow ("Shallow water", Color) = (0.56, 0.8, 1, 1)
        _Milk ("Milky foam", Color) = (0.95, 0.96, 0.95, 1)
        _Cream ("Cream rim", Color) = (0.99, 0.88, 0.58, 1)
        _Clarity ("Clarity (1 = see-through centre)", Range(0, 1)) = 0.45
        _Flow ("Wave phase (CalypsoOrb)", Float) = 0
        _Speak ("Speaking", Range(0, 1)) = 0
        _Listen ("Listening", Range(0, 1)) = 0
        _Think ("Thinking", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Deep, _Shallow, _Milk, _Cream; half _Clarity; float _Flow; half _Speak, _Listen, _Think;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half3 normalWS : TEXCOORD1; float3 positionOS : TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float t = _Time.y; float3 n = normalize(v.normalOS);
                // the shell breathes; it quivers as she talks, and leans in a touch toward the person as they speak
                float3 camOS = TransformWorldToObject(_WorldSpaceCameraPos);
                float wobble = .004 * sin(t * 1.3)
                    + _Speak * .007 * sin(dot(n, float3(3.1, 4.7, 2.3)) * 2.6 + t * 11) * (.6 + .4 * sin(t * 4.3 + n.y * 3))
                    + _Listen * .012 * saturate(dot(n, normalize(camOS))) * (.5 + .5 * sin(t * 7));
                float3 p = v.positionOS.xyz + n * wobble;
                o.positionOS = p * 2;   // the unit ball
                o.positionWS = TransformObjectToWorld(p); o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            // Swirling water at a point in the unit ball: twisted round the vertical axis, crossed sine swells warped into
            // each other, rings out from the centre as she talks and in from the person's side as they speak.
            float Water(float3 q, float3 toEye, float flow, float t)
            {
                float a = flow * .35 + q.y * 1.7 + length(q.xz) * 1.2, s, c; sincos(a, s, c);
                float3 r = float3(c * q.x - s * q.z, q.y, s * q.x + c * q.z);
                float h = sin(r.x * 3.6 + sin(r.y * 3.1 + flow * .9) * 1.5 + flow * 1.1)
                        + .65 * sin(r.z * 4.9 - flow * .8 + sin(r.x * 2.7 - flow * .6) * 1.3)
                        + .3 * sin((r.x + r.y) * 8.3 + flow * 1.9);
                float rad = length(q);
                h += _Speak * 1.1 * sin(rad * 13 - t * 8.5) * saturate(1.1 - rad);
                float d = length(q - toEye);
                h += _Listen * 1.0 * sin(d * 15 - t * 9) * saturate(1.3 - d);
                return h;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float t = _Time.y, flow = _Flow;
                float3 view = GetWorldSpaceNormalizeViewDir(i.positionWS);
                half3 n = normalize(i.normalWS);
                half cosV = saturate(dot(n, view)), rim = 1 - cosV, F = pow(rim, 3);

                // ---- the water inside, at three depths along the eye ray
                float3 eyeOS = TransformWorldToObject(_WorldSpaceCameraPos) * 2;
                float3 p = i.positionOS, rd = normalize(p - eyeOS), toEye = normalize(eyeOS);
                float chord = max(-2 * dot(p, rd), 0);
                // the blue gathers up and to the left, and drifts slowly round
                float sd, cd; sincos(flow * .07, sd, cd);
                float3 gather = normalize(float3(-.55 * cd - .2 * sd, .75, .35 * cd));
                half blue = 0, sheen = 0, weight = 0;
                [unroll] for (int k = 0; k < 3; k++)
                {
                    float f = (k + .5) / 3, w = 1.3 - f;   // nearer layers count more
                    float3 q = p + rd * chord * f;
                    float h = Water(q, toEye, flow, t);
                    half pool = saturate(.45 + .7 * dot(q, gather));
                    blue += smoothstep(.15, 1.5, h) * lerp(.35, 1, pool) * w;
                    sheen += exp(-abs(h - 1.05) * 9) * w;   // thin bright crest lines
                    weight += w;
                }
                blue /= weight; sheen /= weight;
                half deep = saturate(blue * 2.8 - .12);
                half3 water = lerp(_Milk.rgb, lerp(_Shallow.rgb, _Deep.rgb, saturate(deep * 1.3 - .2)), deep);
                water += sheen * .4;
                half milk = saturate(chord * .5);   // thicker through the middle: milkier

                // ---- the shell: thin-film colour toward the rim (blue up-left, cream down-right), environment, highlights
                float3 key = normalize(float3(-.55, .7, .45));
                half side = saturate(.5 + .6 * dot(n, key));
                half3 film = .5 + .5 * cos(6.2832 * (cosV * 1.6 + n.y * .3 + flow * .02 + float3(0, .33, .67)));
                half3 rimCol = lerp(lerp(_Cream.rgb, _Shallow.rgb, side), film, .22);
                half3 env = GlossyEnvironmentReflection(reflect(-view, n), i.positionWS, .05, 1);
                Light sun = GetMainLight();
                float3 refl = reflect(-view, n);
                half spec = pow(saturate(dot(refl, sun.direction)), 300) * 3 * Luminance(sun.color)
                          + pow(saturate(dot(refl, key)), 90) * 1.4 + pow(saturate(dot(refl, normalize(float3(.5, -.45, .6)))), 60) * .35;

                half edge = saturate(pow(rim, 2) * 1.5);
                half3 c = lerp(water, rimCol, edge) + env * F * .4 + spec;
                c = lerp(c, _Cream.rgb, (1 - side) * saturate(rim * 1.6 - .2) * .45);   // the warm glow inside the lower right
                // self-lit, but it takes some of the place's light, so it sits in the scene by day and still glows at night
                half amb = Luminance(SampleSH(n)) + Luminance(sun.color) * .3;
                c *= lerp(.78, 1.08, saturate(amb)) * (1 + .3 * _Speak + .18 * _Listen + .1 * _Think);

                half alpha = saturate((1 - _Clarity) * (.55 + .6 * milk) + deep * .55 + sheen * .3 + edge * .7 + spec + .12 * _Speak);
                return half4(c, alpha);
            }
            ENDHLSL
        }
    }
}

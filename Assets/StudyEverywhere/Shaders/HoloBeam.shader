// The holodeck's projector light: the cone from the table's ring up to the model, and the glowing disc on the glass.
// Cone (_Disc 0): brightest at the ring, fading as it rises (object-space height _YMin.._YMax), faint where the surface
// is seen edge-on so it reads as a soft volume, with slow light streaks running round it and bands drifting upward.
// Disc (_Disc 1, on a quad): a soft pool of light with fine rings and a slowly turning dashed ring. Added light only.
Shader "StudyEverywhere/Holo Beam"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (0.25, 0.75, 1.1, 0.5)
        _YMin ("Cone bottom (object y)", Float) = 0
        _YMax ("Cone top (object y)", Float) = 1
        [Toggle] _Disc ("Projector disc (on a quad)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend One One
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
                half4 _Color;
                float _YMin, _YMax, _Disc;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 posOS : TEXCOORD0; float3 posWS : TEXCOORD1; float3 normalWS : TEXCOORD2; float2 uv : TEXCOORD3; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                ZERO_INITIALIZE(Varyings, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.posOS = v.positionOS.xyz;
                o.posWS = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = TransformWorldToHClip(o.posWS);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float t = _Time.y;
                half light;
                if (_Disc > .5)
                {
                    float2 d = i.uv * 2 - 1; float r = length(d), ang = atan2(d.y, d.x);
                    half pool = exp(-r * r * 4) * .7;
                    half rings = 0;
                    rings += exp(-pow((r - .42) * 60, 2)) * .5;
                    rings += exp(-pow((r - .66) * 70, 2)) * .35;
                    rings += exp(-pow((r - .9) * 45, 2)) * .6;
                    half dash = step(.5, frac(ang / 6.2831853 * 36 + t * .04)) * exp(-pow((r - .78) * 50, 2)) * .7;
                    light = (pool + rings + dash) * saturate((1 - r) * 8);
                }
                else
                {
                    float h = saturate((i.posOS.y - _YMin) / max(_YMax - _YMin, 1e-4));
                    float3 V = normalize(GetCameraPositionWS() - i.posWS);
                    half facing = abs(dot(normalize(i.normalWS), V));
                    half body = facing * facing;
                    half fade = pow(1 - h, 1.4) * smoothstep(0, .04, h) * (1 - smoothstep(.85, 1, h));
                    float ang = atan2(i.posOS.z, i.posOS.x);
                    half streaks = .7 + .3 * sin(ang * 23 + t * .35) * sin(ang * 9 - t * .22);
                    half bands = .85 + .15 * sin(h * 40 - t * 2.2);
                    light = body * fade * streaks * bands;
                }
                return half4(_Color.rgb * light * _Color.a, 0);
            }
            ENDHLSL
        }
    }
}

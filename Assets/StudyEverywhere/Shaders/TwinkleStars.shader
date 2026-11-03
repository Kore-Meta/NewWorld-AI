// Sparkling stars over a baked sky: one small camera-facing sprite per star, far away, each twinkling at its own
// pace (a slow swell and a faster flicker). StudyEverywhere.TwinkleStars builds the mesh: a star's centre is its
// position, uv its corner (-1..1), uv2 (size in metres, phase, speed, brightness) and its colour the vertex colour.
// _Visible fades the whole field (night only in the places with a daytime sky).
Shader "StudyEverywhere/Twinkle Stars"
{
    Properties
    {
        _Visible ("Visible", Range(0, 1)) = 1
        _Gain ("Brightness", Range(0, 4)) = 1.4
        _Sparkle ("Sparkle (sharp glints over the twinkle)", Range(0, 1)) = 0
        [NoScaleOffset] _Mask ("Sky mask (R: open sky)", Cube) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-50" "RenderType" = "Transparent" "IgnoreProjector" = "True" "DisableBatching" = "True" }
        Blend One One
        ColorMask RGB // Stars add light, never alpha to the XR compositor's opaque background.
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            half _Visible, _Gain, _Sparkle; samplerCUBE _Mask;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 star : TEXCOORD1; half4 color : COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; half3 color : COLOR; float3 dir : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 view = UnityObjectToViewPos(v.vertex.xyz);
                view.xy += v.uv * v.star.x;                       // face the camera
                o.pos = mul(UNITY_MATRIX_P, float4(view, 1));
                o.uv = v.uv;
                o.dir = v.vertex.xyz;   // the star's direction from the field's centre (the place's bake eye)
                float t = _Time.y * v.star.z + v.star.y;
                half twinkle = (0.55 + 0.45 * sin(t)) * (0.8 + 0.2 * sin(t * 2.7 + v.star.y * 3.1));
                half glint = pow(saturate(0.5 + 0.5 * sin(t * 2.3 + v.star.y * 5.1)), 10);   // now and then a star flares, each at its own moment
                o.color = v.color.rgb * v.star.w * (twinkle + _Sparkle * 1.8 * glint) * _Visible * _Gain;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float r2 = dot(i.uv, i.uv);
                half core = exp(-r2 * 9);
                half spikes = exp(-abs(i.uv.x) * 26) * exp(-abs(i.uv.y) * 2.5) + exp(-abs(i.uv.y) * 26) * exp(-abs(i.uv.x) * 2.5);
                half k = saturate(core + spikes * 0.35) * saturate(1 - r2);
                k *= texCUBE(_Mask, i.dir).r;   // never over trees, hills or the lake
                return half4(i.color * k, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

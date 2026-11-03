// A shooting star over a baked night sky: StudyEverywhere.ShootingStars sets its tail and head (world points on a far
// sphere round _Centre) each frame; this lays the ribbon between them facing the camera, a fine white-blue line that
// is brightest and widest at the head and fades out along the tail, and clips it by the sky mask (R: open sky), so it
// never shows over a roof, a tree or a hill. _Life fades it in and out over its flight.
Shader "StudyEverywhere/Shooting Star"
{
    Properties
    {
        _Gain ("Brightness", Range(0, 2)) = 1.6
        _Width ("Width at the head (metres at the sky's distance)", Float) = 3.4
        [NoScaleOffset] _Mask ("Sky mask (R: open sky)", Cube) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-49" "RenderType" = "Transparent" "IgnoreProjector" = "True" "DisableBatching" = "True" }
        Blend One One
        ColorMask RGB // adds light, never alpha to the XR compositor's opaque background
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            half _Gain, _Width, _Life; float4 _Head, _Tail, _Centre; samplerCUBE _Mask;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float3 dir : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 p = lerp(_Tail.xyz, _Head.xyz, v.uv.x);
                float3 along = _Head.xyz - _Tail.xyz + 1e-4, side = normalize(cross(along, p - _WorldSpaceCameraPos));
                p += side * v.uv.y * _Width * (.25 + .75 * v.uv.x);   // thin at the tail, fullest at the head
                o.pos = UnityWorldToClipPos(p);
                o.uv = v.uv;
                o.dir = p - _Centre.xyz;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half across = exp(-i.uv.y * i.uv.y * 5);           // a soft line
                half fade = i.uv.x * i.uv.x * i.uv.x;              // the tail dies away behind the head
                half head = exp(-(1 - i.uv.x) * (1 - i.uv.x) * 900); // a brighter spark at the very front
                half3 c = lerp(half3(.75, .85, 1), half3(1, .98, .94), i.uv.x) * across * (fade + head * 1.2);
                c *= texCUBE(_Mask, i.dir).r * _Life * _Gain;
                return half4(min(c, .98), 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

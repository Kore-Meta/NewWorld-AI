// The thin shell of air round Orbit's Earth, seen edge-on: a slightly larger sphere drawn additively, bright only
// toward its rim. Blue where the sun lights it, warm along the terminator, and when the sun is straight behind the
// Earth (night) a ring all the way round, orange inside and blue outside. Sun direction from OrbitEarth.
Shader "StudyEverywhere/Orbit Atmosphere"
{
    Properties
    {
        _SunDir ("Toward the sun (world)", Vector) = (0, 0.5, -1, 0)
        _Strength ("Strength", Range(0, 4)) = 1.3
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend One One
        ZWrite Off
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _SunDir; half _Strength;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float3 n : TEXCOORD0; float3 view : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.n = UnityObjectToWorldNormal(v.normal);
                o.view = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 n = normalize(i.n), v = normalize(i.view), s = normalize(_SunDir.xyz);
                float edge = 1 - saturate(dot(n, v));             // 0 facing us, 1 at the rim
                half rim = pow(edge, 5) * smoothstep(1.0, 0.9, edge * 1.02);
                half day = smoothstep(-0.25, 0.35, dot(n, s));
                half term = smoothstep(-0.25, 0.0, dot(n, s)) * (1 - smoothstep(0.0, 0.3, dot(n, s)));
                half back = pow(saturate(dot(-v, s)), 6);          // the sun behind the Earth: a ring
                half3 c = half3(0.25, 0.5, 1.2) * day * rim;
                c += half3(1.4, 0.55, 0.2) * term * rim * 0.7;
                // the ring: only the last sliver of the shell, orange inside turning blue at the very edge
                half ring = pow(edge, 9) * smoothstep(1.0, 0.92, edge * 1.02);
                c += lerp(half3(1.3, 0.45, 0.12), half3(0.35, 0.6, 1.3), smoothstep(0.86, 0.97, edge)) * back * ring * 2.2;
                return half4(c * _Strength, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

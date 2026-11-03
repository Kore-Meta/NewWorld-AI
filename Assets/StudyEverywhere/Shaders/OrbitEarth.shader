// Orbit's Earth: NASA's Blue Marble lit by the sun, Black Marble city lights on the night side, the cloud layer
// drifting a little faster than the ground, the sun's glint on the sea, and blue haze toward the limb. Unlit: the sun
// direction comes from StudyEverywhere.OrbitEarth (the true sun for each time of day, not the room's light, which is
// the Moon's at night). The mesh is an equirectangular UV sphere; _CloudShift slides the clouds round the axis.
Shader "StudyEverywhere/Orbit Earth"
{
    Properties
    {
        [NoScaleOffset] _DayTex ("Day (Blue Marble)", 2D) = "black" {}
        [NoScaleOffset] _NightTex ("Night lights (Black Marble)", 2D) = "black" {}
        [NoScaleOffset] _CloudTex ("Clouds", 2D) = "black" {}
        _SunDir ("Toward the sun (world)", Vector) = (0, 0.5, -1, 0)
        _SunColor ("Sunlight", Color) = (1, 0.97, 0.92, 1)
        _CloudShift ("Cloud shift (0..1 of a turn)", Float) = 0
        _NightGain ("City light strength", Range(0, 4)) = 1.6
        _Exposure ("Exposure", Range(0, 4)) = 1.15
        _Haze ("Air at the limb", Range(0, 1)) = 1
        _Earthshine ("Faint light on the dark side", Range(0, 0.2)) = 0.035
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _DayTex, _NightTex, _CloudTex;
            float4 _SunDir; half4 _SunColor; float _CloudShift; half _NightGain, _Exposure, _Haze, _Earthshine;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float3 n : TEXCOORD1; float3 view : TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.n = UnityObjectToWorldNormal(v.normal);
                o.view = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 n = normalize(i.n), v = normalize(i.view), s = normalize(_SunDir.xyz);
                float ndl = dot(n, s), mu = saturate(dot(n, v));
                half3 day = tex2D(_DayTex, i.uv).rgb;
                half3 lights = tex2D(_NightTex, i.uv).rgb;
                half cloud = tex2D(_CloudTex, float2(frac(i.uv.x + _CloudShift), i.uv.y)).r;
                // light: a soft terminator, warm where the sun is low
                half lit = smoothstep(-0.06, 0.25, ndl) * saturate(ndl + 0.15);
                half twilight = smoothstep(-0.12, 0.05, ndl) * (1 - smoothstep(0.05, 0.3, ndl));
                half3 sun = lerp(_SunColor.rgb, half3(1.3, 0.62, 0.32), twilight * 0.7);
                half3 c = day * sun * lit * 1.25;
                // the sea (blue, dark) catches the sun
                half sea = saturate((day.b - max(day.r, day.g)) * 6) * (1 - cloud);
                float3 r = reflect(-v, n);
                c += sun * sea * pow(saturate(dot(r, s)), 60) * 0.8 * lit;
                // clouds over everything, lit by the sun, faintly by the cities beneath
                c = lerp(c, sun * (lit * 1.05 + twilight * 0.15), cloud * 0.95);
                // city lights where it is dark, dimmed under clouds
                half dark = smoothstep(0.08, -0.18, ndl);
                // (Black Marble carries a faint moonlit land and sea under the lights: keep only the lights themselves)
                half glow = saturate((max(lights.r, max(lights.g, lights.b)) - 0.16) * 1.6);
                c += glow * glow * half3(1.0, 0.76, 0.45) * _NightGain * dark * (1 - cloud * 0.8);
                c += day * _Earthshine * half3(0.75, 0.85, 1.25) * dark;   // a whisper of moonlight (or earthshine) on the dark side
                // air toward the limb: blue on the day side, warm along the terminator
                half haze = pow(1 - mu, 3) * _Haze;
                c = c * (1 - 0.35 * haze) + haze * (half3(0.2, 0.42, 0.95) * lit + half3(1.0, 0.45, 0.18) * twilight * 0.6);
                return half4(c * _Exposure, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

// Drifting clouds over a place's baked sky (Haven, Solace): a far dome drawn with alpha, its clouds from two octaves
// of a tileable noise texture projected on a flat cloud deck, sliding with the wind. _Coverage sets how much of the
// sky they hide (0 clear, 1 overcast); StudyEverywhere.CloudCover sets it from the menu and lights the clouds from the
// clock: bright and warm on the sun's side, shaded underneath, grey-blue by moonlight. They thin out toward the
// horizon, where the baked hills and trees are.
Shader "StudyEverywhere/Cloud Layer"
{
    Properties
    {
        [NoScaleOffset] _Noise ("Tileable noise (R broad, G fine)", 2D) = "grey" {}
        _Coverage ("Coverage", Range(0, 1)) = 0.35
        _Scale ("Cloud size", Float) = 0.9
        _Wind ("Wind (xy)", Vector) = (0.006, 0.002, 0, 0)
        _SunDir ("Toward the sun or moon", Vector) = (0, 0.5, 1, 0)
        _Lit ("Lit colour", Color) = (1, 0.97, 0.92, 1)
        _Shade ("Shade colour", Color) = (0.62, 0.66, 0.74, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.92
        _Horizon ("Fade above the horizon", Range(0.01, 0.5)) = 0.12
        [NoScaleOffset] _Mask ("Sky mask (R: open sky)", Cube) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-200" "RenderType" = "Transparent" "IgnoreProjector" = "True" "DisableBatching" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha   // alpha stays opaque, so passthrough never shows through
        ZWrite Off
        Cull Front
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _Noise; samplerCUBE _Mask; half _Coverage, _Scale, _Opacity, _Horizon; float4 _Wind, _SunDir; half4 _Lit, _Shade;

            struct appdata { float4 vertex : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;   // a unit dome round the object's origin
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float3 d = normalize(i.dir);
                if (d.y <= 0.0) return 0;
                float2 p = d.xz / (d.y + 0.08) * _Scale;
                float t = _Time.y;
                half broad = tex2D(_Noise, p * 0.11 + _Wind.xy * t).r;
                half fine = tex2D(_Noise, p * 0.37 - _Wind.yx * t * 1.7 + 0.37).g;
                half n = broad * 0.72 + fine * 0.38;
                half edge = 1.02 - _Coverage * 0.62;
                half density = smoothstep(edge - 0.12, edge + 0.1, n);
                density *= smoothstep(0.0, _Horizon, d.y) * texCUBE(_Mask, d).r;   // not over the trees and hills
                if (density < 0.003) return 0;
                // light: brighter toward the sun or moon, a silver lining at thin edges, darker in thick middles
                float3 s = normalize(_SunDir.xyz);
                half toward = saturate(dot(d, s) * 0.5 + 0.5);
                half thick = smoothstep(edge, edge + 0.35, n);
                half3 c = lerp(_Lit.rgb, _Shade.rgb, thick * 0.75);
                c += _Lit.rgb * pow(toward, 6) * (1 - thick) * 0.6;
                return half4(c, density * _Opacity);
            }
            ENDCG
        }
    }
    Fallback Off
}

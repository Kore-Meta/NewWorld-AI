// Copies a shared screen's stream with a light unsharp mask (ScreenSurface), so desktop text keeps a little more edge
// after the headset's lenses and resampling soften it. In Resources so it ships without a material referencing it.
Shader "Hidden/NewWorld/ScreenSharpen"
{
    Properties
    {
        _MainTex ("Stream", 2D) = "white" {}
        _Amount ("Amount", Float) = 0.3
    }
    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Amount;

            fixed4 frag (v2f_img i) : SV_Target
            {
                float2 t = _MainTex_TexelSize.xy;
                fixed3 c = tex2D(_MainTex, i.uv).rgb;
                fixed3 n = tex2D(_MainTex, i.uv + float2(t.x, 0)).rgb + tex2D(_MainTex, i.uv - float2(t.x, 0)).rgb
                         + tex2D(_MainTex, i.uv + float2(0, t.y)).rgb + tex2D(_MainTex, i.uv - float2(0, t.y)).rgb;
                return fixed4(saturate(c + (c * 4 - n) * _Amount), 1);
            }
            ENDCG
        }
    }
}

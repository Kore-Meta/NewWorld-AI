// Coffee steam: soft round wisps for a particle system, tinted and faded by the particle's vertex colour.
// Procedural falloff, no texture; alpha-blended, no depth write, cheap enough for a handful of particles on Quest.
Shader "StudyEverywhere/Steam"
{
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                o.color = i.color;
                o.uv = i.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 d = i.uv * 2 - 1;
                half wisp = saturate(1 - dot(d, d));
                return half4(i.color.rgb, i.color.a * wisp * wisp);
            }
            ENDHLSL
        }
    }
}

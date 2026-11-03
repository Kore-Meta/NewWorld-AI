// The holodeck missions' fire and smoke: a soft round puff for a particle system, drawn without a texture. One blend
// (premultiplied alpha) does both: _Glow 1 adds light (rocket flames, the plasma of re-entry, the Sun), _Glow 0 covers
// what is behind it (smoke, steam, sea spray). No depth write.
Shader "StudyEverywhere/Mission Puff"
{
    Properties
    {
        _Glow ("Glow (1 adds light, 0 covers)", Range(0, 1)) = 1
        _Soft ("Edge softness", Range(1, 12)) = 4
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend One OneMinusSrcAlpha
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
                half _Glow, _Soft;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                ZERO_INITIALIZE(Varyings, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 d = i.uv * 2 - 1;
                float r2 = dot(d, d);
                half a = saturate(exp(-r2 * _Soft) * saturate(1 - r2) * 1.15) * i.color.a;
                return half4(i.color.rgb * a, a * (1 - _Glow));
            }
            ENDHLSL
        }
    }
}

// The lobby's demonstration hands, in the look of Meta's own system hands: soft grey and see-through, a little lighter
// where they face the light, with a bright white line round their outline (a rim from the view angle). A depth-only
// pass first, so the translucent hand shows only its nearest surface (no fingers seen through the palm). Unlit.
Shader "StudyEverywhere/Demo Hand"
{
    Properties
    {
        _Color ("Grey (alpha: how solid)", Color) = (0.62, 0.64, 0.68, 0.6)
        _Rim ("Outline", Color) = (1, 1, 1, 1)
        _RimPower ("Outline sharpness", Range(1, 12)) = 4.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half4 _Color, _Rim; half _RimPower;
        CBUFFER_END
        struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
        struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 posWS : TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
        Varyings vert(Attributes v)
        {
            Varyings o;
            UNITY_SETUP_INSTANCE_ID(v);
            ZERO_INITIALIZE(Varyings, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.posWS = TransformObjectToWorld(v.positionOS.xyz);
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            o.positionCS = TransformWorldToHClip(o.posWS);
            return o;
        }
        ENDHLSL

        // 1: depth only
        Pass
        {
            Name "DepthPrepass"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            ZWrite On
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            half4 frag(Varyings i) : SV_Target { return 0; }
            ENDHLSL
        }
        // 2: the see-through grey with its white outline, nearest surface only
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha   // alpha stays opaque, so passthrough never shows through
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float3 n = normalize(i.normalWS), view = normalize(GetCameraPositionWS() - i.posWS);
                half facing = saturate(dot(n, view));
                half shade = .78 + .22 * saturate(dot(n, normalize(float3(.3, .9, -.2))));
                half rim = saturate(pow(1 - facing, _RimPower) * 1.6);
                return half4(lerp(_Color.rgb * shade, _Rim.rgb, rim), lerp(_Color.a, 1, rim));
            }
            ENDHLSL
        }
    }
}

// The holodeck's wireframe: glowing cyan lines along a model's edges. The Orbit builder turns each mesh's edges into a
// mesh of thin quads (each vertex holds its own end of the edge, the other end in TEXCOORD1 and its side, -1 or 1, in
// TEXCOORD0.x); here each quad is opened sideways, across the line as seen from the eye, so the lines keep a steady
// width from any angle and never thinner than a few pixels. Added light, no depth write: the back edges show through
// like light. No geometry shader (the Quest's Vulkan path does not need one), one pass.
Shader "StudyEverywhere/Holo Wire"
{
    Properties
    {
        [HDR] _Color ("Colour", Color) = (0.3, 0.85, 1.2, 1)
        _Width ("Half width (m)", Float) = 0.0007
        _Angle ("Minimum half width (radians from the eye)", Float) = 0.0011
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
                float _Width, _Angle;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; float3 other : TEXCOORD1; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float side : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                ZERO_INITIALIZE(Varyings, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 a = TransformObjectToWorld(v.positionOS.xyz), b = TransformObjectToWorld(v.other);
                float3 view = a - GetCameraPositionWS();
                float3 across = cross(b - a, view);
                float len = length(across);
                across = len > 1e-10 ? across / len : float3(0, 1, 0);
                float w = max(_Width, length(view) * _Angle);
                o.positionCS = TransformWorldToHClip(a + across * v.uv.x * w);
                o.side = v.uv.x;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half k = saturate(1 - abs(i.side));
                half glow = k * k * (3 - 2 * k);   // bright core, soft edge (no hard pixel stairs)
                return half4(_Color.rgb * glow * _Color.a, 0);
            }
            ENDHLSL
        }
    }
}

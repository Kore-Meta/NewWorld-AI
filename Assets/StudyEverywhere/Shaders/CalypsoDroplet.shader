// Calypso's droplets, for the particle system round her orb (CalypsoOrb): each a tiny water bubble, a bright rim with a
// clear middle and a glint up to the left, in the particle's colour (its alpha fades it). Drawn procedurally on the
// particle quad, no texture. Alpha blended, so they show against a bright sky as well as at night.
Shader "StudyEverywhere/Calypso Droplet"
{
    Properties { }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
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

            struct Attributes { float4 positionOS : POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; half4 color : COLOR; float2 uv : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz); o.color = v.color; o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 p = i.uv * 2 - 1; float r = length(p);
                half edge = 1 - smoothstep(.82, 1, r);
                half ring = smoothstep(.45, .85, r) * edge;
                half glint = exp(-dot(p - float2(-.35, .35), p - float2(-.35, .35)) * 28);
                half3 c = i.color.rgb * (.75 + .5 * ring) + glint;
                half a = saturate(ring * .8 + .18 * edge + glint) * i.color.a;
                return half4(c, a);
            }
            ENDHLSL
        }
    }
}

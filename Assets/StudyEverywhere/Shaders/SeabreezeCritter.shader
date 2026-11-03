// Seabreeze's small creatures (turtle hatchlings, ghost crabs): each is one mesh, its limbs moved here so it costs a
// single draw. A limb's vertices carry their joint (uv1.xyz, object space) and phase in the stroke (uv1.w), the axis they
// swing about (uv2.xyz) and how far (uv2.w, radians); a second phase and amplitude (uv3.xy) are used while swimming. The
// creature's script advances _Stroke (radians) while it moves and sets _Swim (0 on land, 1 in the water) per renderer.
// Lit like the sand; below the sea level the lagoon tints it, as it does the sand.
Shader "StudyEverywhere/Seabreeze Critter"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _Gloss ("Wet gloss", Range(0, 1)) = 0.4
        _Stroke ("Stroke (radians)", Float) = 0
        _Swim ("Swimming", Range(0, 1)) = 0
        _Absorb ("Water absorption per metre (rgb)", Vector) = (2.6, 0.75, 0.5, 0)
        _Lagoon ("Lagoon colour", Color) = (0.05, 0.42, 0.45, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST, _Absorb;
                half4 _Lagoon;
                half _Gloss;
                float _Stroke, _Swim;
            CBUFFER_END
            float _SeaLevel;

            struct A { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; float4 joint : TEXCOORD1; float4 axis : TEXCOORD2; float4 swim : TEXCOORD3; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 p : TEXCOORD1; float3 n : TEXCOORD2; UNITY_VERTEX_OUTPUT_STEREO };

            float3 Rotate(float3 v, float3 axis, float a)
            {
                float s = sin(a), c = cos(a);
                return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1 - c);
            }

            V vert(A i)
            {
                V o; UNITY_SETUP_INSTANCE_ID(i); ZERO_INITIALIZE(V, o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 pos = i.positionOS.xyz, nrm = i.normalOS;
                float amp = lerp(i.axis.w, i.swim.y, _Swim);
                if (amp != 0)
                {
                    float phase = lerp(i.joint.w, i.swim.x, _Swim) * 6.2832;
                    float a = amp * sin(_Stroke + phase);
                    float3 axis = normalize(i.axis.xyz);
                    pos = i.joint.xyz + Rotate(pos - i.joint.xyz, axis, a);
                    nrm = Rotate(nrm, axis, a);
                }
                o.p = TransformObjectToWorld(pos);
                o.positionCS = TransformWorldToHClip(o.p);
                o.n = TransformObjectToWorldNormal(nrm);
                o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                return o;
            }

            half4 frag(V i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv).rgb;
                float3 n = normalize(i.n), v = normalize(GetWorldSpaceViewDir(i.p));
                Light sun = GetMainLight(TransformWorldToShadowCoord(i.p));
                half3 light = sun.color * sun.shadowAttenuation * saturate(dot(n, sun.direction) * 0.8 + 0.2) + SampleSH(n);
            #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint k = 0; k < count; k++) { Light l = GetAdditionalLight(k, i.p); light += l.color * l.distanceAttenuation * saturate(dot(n, l.direction)); }
            #endif
                half3 c = albedo * light;
                half3 h = normalize(sun.direction + v);
                c += sun.color * sun.shadowAttenuation * pow(saturate(dot(n, h)), 60) * _Gloss;
                float under = _SeaLevel - i.p.y;
                if (under > 0)
                {
                    half3 trans = exp(-_Absorb.rgb * under * 1.5);
                    c = c * trans + _Lagoon.rgb * (SampleSH(float3(0, 1, 0)) + sun.color * 0.3) * (1 - trans);
                }
                return half4(c, 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}

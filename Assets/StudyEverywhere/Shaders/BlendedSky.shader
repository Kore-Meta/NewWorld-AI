// A baked place's sky across the day: two cubemaps of the same view at neighbouring times (say afternoon and sunset),
// cross-faded by _Blend. StudyEverywhere.TimeOfDay picks the pair and the blend from its clock.
// With a lake (Haven: _WaterOn = 1), the water in the baked view comes alive: where the place's mask cubemap is green
// (the lake, as seen from the bake eye) the shader finds where the view ray meets the water plane, ripples its normal
// with drifting noise, and reflects the sky there, darker and blurrier by night, with the sun's (or moon's) glitter.
// With _BakedMirror (New York: its ponds 100 m below, ringed with trees and towers), the reflection is the bake's own
// water, which already mirrors the shores, its look shifted by the moving ripples so the trees and buildings in it
// shimmer and break as on real water.
Shader "StudyEverywhere/Blended Sky"
{
    Properties
    {
        [NoScaleOffset] _SkyA ("Sky (from)", Cube) = "grey" {}
        [NoScaleOffset] _SkyB ("Sky (to)", Cube) = "grey" {}
        _Blend ("Blend", Range(0, 1)) = 0
        _Exposure ("Exposure", Range(0, 4)) = 1
        [Header(Lake)]
        [Toggle(_WATER)] _WaterOn ("Live water", Float) = 0
        [NoScaleOffset] _Mask ("Mask (R sky, G water)", Cube) = "black" {}
        [NoScaleOffset] _Ripples ("Ripple noise", 2D) = "grey" {}
        _WaterLevel ("Water height below the bake eye (negative)", Float) = -6.7
        _RippleScale ("Ripple size", Float) = 0.12
        _RippleStrength ("Ripple strength", Range(0, 1)) = 0.35
        _WaterTint ("Deep water", Color) = (0.015, 0.04, 0.06, 1)
        _SunDir ("Toward the sun or moon", Vector) = (0, 0.3, 1, 0)
        _Glitter ("Glitter colour", Color) = (1.6, 1.1, 0.55, 1)
        _Night ("Night", Range(0, 1)) = 0
        _RippleFar ("Ripples calm with distance", Float) = 0.015
        _BakedMirror ("Ripple the baked reflection", Range(0, 1)) = 0
        _MirrorShift ("Baked reflection shift", Float) = 0.05
    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _WATER
            #include "UnityCG.cginc"

            samplerCUBE _SkyA, _SkyB;
            half _Blend, _Exposure;
            #if _WATER
            samplerCUBE _Mask; sampler2D _Ripples;
            float _WaterLevel, _RippleScale; half _RippleStrength, _Night, _RippleFar, _BakedMirror, _MirrorShift; half4 _WaterTint, _Glitter; float4 _SunDir;
            #endif

            struct appdata { float4 vertex : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos : SV_POSITION; float3 dir : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            half3 Sky(float3 d, float lod)
            {
                half3 a = texCUBElod(_SkyA, float4(d, lod)).rgb;
                return _Blend > 0.002 ? lerp(a, texCUBElod(_SkyB, float4(d, lod)).rgb, _Blend) : a;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half3 a = texCUBE(_SkyA, i.dir).rgb;
                // Skip the second fetch when it would not show (most of the time the clock sits on one key).
                half3 c = _Blend > 0.002 ? lerp(a, texCUBE(_SkyB, i.dir).rgb, _Blend) : a;
                #if _WATER
                float3 d = normalize(i.dir);
                half water = d.y < -0.002 ? texCUBE(_Mask, d).g : 0;
                if (water > 0.01)
                {
                    float t = _WaterLevel / d.y;                       // along the ray to the water plane
                    float2 p = d.xz * t * _RippleScale;
                    float time = _Time.y;
                    float2 w1 = p + float2(0.021, 0.013) * time, w2 = p * 2.7 + float2(-0.017, 0.029) * time;
                    half2 slope = (tex2D(_Ripples, w1).rg - 0.5) + (tex2D(_Ripples, w2).rg - 0.5) * 0.6;
                    half fade = 1 / (1 + t * _RippleFar);                   // far water is a calm mirror
                    float3 n = normalize(float3(slope.x * _RippleStrength * fade, 1, slope.y * _RippleStrength * fade));
                    float3 r = reflect(d, n); r.y = max(r.y, 0.01);
                    half F = 0.02 + 0.98 * pow(1 - abs(d.y), 5);
                    half3 refl = Sky(r, lerp(0.5, 4.0, _Night)) * lerp(0.85, 0.55, _Night);
                    half3 w = lerp(_WaterTint.rgb * lerp(1, 0.25, _Night), refl, saturate(F + 0.12));
                    if (_BakedMirror > 0.001)
                    {
                        // the baked water a ripple's slope away (only while that is still water), each facet a touch
                        // brighter or darker as it tilts toward or away from the viewer
                        float3 d2 = normalize(d + float3(slope.x, 0, slope.y) * _MirrorShift * fade);
                        half3 baked = lerp(c, Sky(d2, 0), saturate(texCUBE(_Mask, d2).g * 2 - 0.5));
                        half facet = 1 + (slope.x * d.x + slope.y * d.z) * _RippleStrength * fade * 2;
                        w = lerp(w, baked * facet, _BakedMirror);
                    }
                    float3 s = normalize(_SunDir.xyz);
                    half glint = pow(saturate(dot(r, s)), 300) * 6 + pow(saturate(dot(r, s)), 60) * 0.5;
                    w += _Glitter.rgb * glint * lerp(1, 0.25, _Night) * (0.5 + 0.5 * tex2D(_Ripples, p * 5 + time * 0.05).b);
                    c = lerp(c, w, water * lerp(0.85, 1, _BakedMirror));
                }
                #endif
                return half4(c * _Exposure, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}

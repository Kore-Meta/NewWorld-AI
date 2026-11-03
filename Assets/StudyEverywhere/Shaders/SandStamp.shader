// Draws into Seabreeze's writing map (SandWriting): one pass per frame reads the old map and writes the new. Each of up to
// four finger strokes this frame (_From, _To: world x, z of a segment; w of _From its strength) presses a groove (negative
// height) about a fingertip wide, pushing up a low lip either side; and wherever the lagoon has run over the sand (the
// beach's height, from the same formula as SeabreezeBeach.Sand, below _SeaLevel) the writing slowly smooths away.
Shader "Hidden/StudyEverywhere/Sand Stamp"
{
    Properties { _MainTex ("Previous", 2D) = "black" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _Rect;            // x, z of the patch's corner, its size, and the seconds since the last pass
            float4 _From[4], _To[4]; // strokes: x, z, (strength in _From.w)
            float _Radius, _SeaLevel;

            struct A { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            V vert(A i) { V o; o.positionCS = TransformObjectToHClip(i.positionOS.xyz); o.uv = i.uv; return o; }

            float Sand(float2 p)
            {
                float shore = 1 + 0.0055 * p.x * p.x + 0.0000035 * p.x * p.x * p.x * p.x;
                float d = p.y - shore;
                if (d >= 0) return max(-4, -0.06 * d - 0.0035 * d * d);
                float up = -d;
                return 0.03 * up + 0.175 * (1 - exp(-up / 5));
            }

            float frag(V i) : SV_Target
            {
                float h = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).r;
                float2 p = _Rect.xy + i.uv * _Rect.z;
                // The wash: under water the marks fade (fast enough that a wave or two erases them).
                if (Sand(p) < _SeaLevel) h *= exp(-_Rect.w * 1.2);
                for (int k = 0; k < 4; k++)
                {
                    float s = _From[k].w; if (s <= 0) continue;
                    float2 a = _From[k].xy, b = _To[k].xy, ab = b - a;
                    float t = saturate(dot(p - a, ab) / max(dot(ab, ab), 1e-8));
                    float d = length(p - (a + ab * t)) / _Radius;
                    float groove = -s * exp(-d * d * 1.6);
                    float lip = 0.35 * s * exp(-(d - 1.5) * (d - 1.5) * 3);
                    h = min(h, groove);
                    if (h > -0.05) h = max(h, lip);
                }
                return h;
            }
            ENDHLSL
        }
    }
}

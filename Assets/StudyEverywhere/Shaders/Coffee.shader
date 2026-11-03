// Coffee in a cup: the liquid volume (the cup's inside, closed at the rim) cut by a level plane that CoffeeLiquid keeps
// horizontal in the world and lets wobble when the cup moves. Back faces below the plane are drawn as the surface,
// shaded where the view ray meets the plane: dark coffee, a crema ring that thickens toward the wall, slow ripples,
// a sharp highlight from the sun and a little Fresnel sheen. Level by default; CoffeeLiquid tilts it via _PlaneNormal.
Shader "StudyEverywhere/Coffee"
{
    Properties
    {
        _Coffee ("Coffee", Color) = (0.11, 0.05, 0.022, 1)
        _Crema ("Crema", Color) = (0.56, 0.37, 0.2, 1)
        _Radius ("Surface radius (m)", Float) = 0.043
        _FillHeight ("Fill height above the cup base (m)", Float) = 0.081
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+10" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Coffee;
                half4 _Crema;
                float _Radius;
                float _FillHeight;
            CBUFFER_END
            float4 _PlaneNormal;   // world surface normal from CoffeeLiquid (up, tilted by the slosh); zero means level

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; };

            Varyings vert(Attributes i)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(i.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(i.normalOS);
                return o;
            }

            half4 frag(Varyings i, FRONT_FACE_TYPE face : FRONT_FACE_SEMANTIC) : SV_Target
            {
                // The surface passes through the cup axis at the fill height (the volume shares the cup's space).
                float3 fill = TransformObjectToWorld(float3(0, _FillHeight, 0));
                float3 n = dot(_PlaneNormal.xyz, _PlaneNormal.xyz) > 1e-6 ? normalize(_PlaneNormal.xyz) : float3(0, 1, 0);
                clip(-dot(i.positionWS - fill, n));
                Light sun = GetMainLight();
                float3 view = normalize(_WorldSpaceCameraPos - i.positionWS);
                if (IS_FRONT_VFACE(face, true, false))
                {
                    // The liquid's side, only seen where it meets the cup wall.
                    half lit = saturate(dot(normalize(i.normalWS), sun.direction));
                    return half4(_Coffee.rgb * (0.35 + 0.65 * lit) * sun.color, 1);
                }

                // The surface: where the view ray crosses the fill plane.
                float3 ray = -view;
                float t = dot(fill - _WorldSpaceCameraPos, n) / min(-1e-4, dot(ray, n));
                float3 s = _WorldSpaceCameraPos + ray * t - fill;
                float r = length(s) / _Radius;
                float2 q = s.xz / _Radius;
                float time = _Time.y;
                float swirl = sin(q.x * 9 + time * 1.7) * cos(q.y * 8 - time * 1.3) + 0.5 * sin((q.x + q.y) * 13 + time * 2.3);
                float3 sn = normalize(n + float3(cos(q.x * 9 + time * 1.7), 0, -sin(q.y * 8 - time * 1.3)) * 0.05);

                half crema = saturate(smoothstep(0.6, 0.97, r) * 0.85 + 0.18 * (swirl * 0.5 + 0.5) * smoothstep(0.35, 0.9, r));
                half3 baseColor = lerp(_Coffee.rgb, _Crema.rgb, crema);
                half lit = saturate(dot(sn, sun.direction));
                half3 h = normalize(sun.direction + view);
                half spec = pow(saturate(dot(sn, h)), 140) * 1.6 * (1 - crema * 0.7);
                half fresnel = pow(1 - saturate(dot(sn, view)), 4) * 0.3;
                half3 color = baseColor * (SampleSH(sn) + sun.color * lit * 0.6) + (spec + fresnel) * sun.color;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}

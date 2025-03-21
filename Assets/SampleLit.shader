Shader "Environment/SampleLit"
{
   Properties
    {
        _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
        _Cutoff("Alpha Cutof",Range(0,1))=0.5 
        _LightIntensity("LightIntensity",range(0.1,1))=0.5

        _Root ("Root", float) = 0
        _WindDir ("Wind Dir", vector) = (1,0,0,0)
        _WindStrength ("Wind Strength", float) = 0.5
        _WindSpeed ("Wind Speed", float) = 0
        _VegetationScale("Vegetation Scale", float) = 0
        [Toggle(_WAVE_ON)] _Wave("_Wave", Int) = 0
        [Toggle(_WORLD_CURVED_ON)] _WORLD_CURVED("Enable World Curved", Int) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline""Queue" = "AlphaTest" "RenderType"="TransparentCutout""ShaderModel"="4.5"}

        Pass
        {
            Tags{"LightMode"="UniversalForward"}

             Stencil {
                Ref 0
                Comp always
                Pass replace
            }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _WORLD_CURVED_ON
            #pragma shader_feature _WAVE_ON
            #pragma multi_compile _ HEIGHT_FOG
            #pragma multi_compile_fog
           
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct a2v
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 pos : SV_POSITION;
                float3 positionWS  : TEXCOORD6;

         
            };
            sampler2D _BaseMap;
            float3 WorldCurvedSize;
            float3 WorldCurvedTarget;
            float3 WorldCurvedDistance;
            float3 _OriginPositionWS;
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Cutoff;         
            float _CurvedToggle;
            float _LightIntensity;

            half _Root;
            half4 _WindDir;
            half _WindStrength;
            half _WindSpeed;
            half _VegetationScale;
            CBUFFER_END

            float rand(half2 co)
            {
              return frac(sin(dot(co.xy ,half2(12.9898,78.233))) * 43758.5453);
            }

             void WindChangeUV(inout float2 uv)
            {
                half2 objPos = UNITY_MATRIX_M[3].xz;
                half wind = (rand(objPos)+_Time.y) *_WindSpeed;
                half2 windDir = normalize(_WindDir);
                half v = dot(windDir,uv)*_VegetationScale;
                uv.x += (saturate(sin(wind*0.2+v))*2+sin(wind+v))*_WindStrength*0.1*saturate(dot(half2(-windDir.y,windDir.x),uv)-_Root);
            }

            v2f vert (a2v v)
            {
                v2f o;
               
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.texcoord, _BaseMap);
                o.positionWS = TransformObjectToWorld(v.vertex);

                //Curved World
                #ifdef _WORLD_CURVED_ON
                    matrix m = UNITY_MATRIX_M;
                    m[1][3] = _OriginPositionWS.y;
                    float3 CWposWS = TransformObjectToWorld(v.vertex);
                    float3 d = ( CWposWS - WorldCurvedTarget) * WorldCurvedSize;
                    float3 YOffset = - dot(d * d , WorldCurvedDistance);
                    CWposWS.y += YOffset;
                    float4 posCS = TransformWorldToHClip(CWposWS);
                #else
                    float4 posCS = TransformObjectToHClip(v.vertex.xyz);
                #endif
                 o.pos = posCS; 
               
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                #if _WAVE_ON
                WindChangeUV(i.uv);
                #endif

                //光照
         Light light = GetMainLight();
                half3 worldNormal = normalize(i.worldNormal);
                half3 worldLightDir = normalize(_MainLightPosition.xyz);
                half3 worldviewDir = normalize(_WorldSpaceCameraPos - i.positionWS);
        half3 diffuse = LightingLambert(light.color.rgb, worldLightDir, worldNormal);
        half3 ambient = _GlossyEnvironmentColor.rgb;

                half4 col = tex2D(_BaseMap, i.uv);
                col.rgb *= _BaseColor.rgb;
                clip(col.a - _Cutoff);
                col.rgb = col.rgb * _LightIntensity * (diffuse + ambient);

                return col;
            }
            ENDHLSL
        }

        Pass 
        {
            Name "ShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _BaseMap;

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Cutoff;         
            float _CurvedToggle;
            float _LightIntensity;

            half _Root;
            half4 _WindDir;
            half _WindStrength;
            half _WindSpeed;
            half _VegetationScale;
            CBUFFER_END

            struct a2v {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            v2f vert(a2v v)
            {
                v2f o = (v2f)0;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }
            real4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_BaseMap, i.uv);
                clip(col.a - 0.001);
                return 0;
            }
            ENDHLSL
        }


    }
    FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}
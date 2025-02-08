Shader "URP/URPShader"
{
    Properties
    {
        [Header(Option)]
        _Color ("Color", Color)=(1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            //Tags{"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            //保证Srp合批预存Cbuffer
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            CBUFFER_END

            v2f vert (a2v v)
            {
                v2f o;
                //主纹理采样
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                //MVP
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                //转换世界坐标法线，位置
                o.worldNormal = TransformObjectToWorldNormal(v.normal);                
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                Light light = GetMainLight();
                //光照分量三件套
                half3 worldNormal = normalize(i.worldNormal);
                half3 worldLightDir = normalize(_MainLightPosition.xyz);
                half3 worldviewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                //漫反射
                half3 diffuse = LightingLambert(light.color.rgb, worldLightDir, worldNormal);
                //环境光
                half3 ambient = _GlossyEnvironmentColor.rgb;
                //采样主纹理
                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*_Color;
                //混合输出
                col.rgb = light.color * (col.rgb * diffuse + ambient);

                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}
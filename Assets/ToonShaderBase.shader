Shader "Unlit/ToonShaderBase"
{
    Properties
    {
        [Header(Tex)]
        [Space(20)]
        _MainTex ("Texture", 2D) = "white" {}
        _RampTex("_RampTex", 2D) = "white" {}
        _IlmTex ("IlmTex", 2D) = "white" {}
        [Header(ShadowColor)]
        [Space(20)]
        _MainColor ("_MainColor", Color)=(1,1,1,1)
        _ShadowColor ("_ShadowColor", Color)=(1,1,1,1)
        _ShadowRange ("_ShadowRange",Range(-1,1)) = 0.5
        [Header(SpecularColor)]
        [Space(20)]
    _SpecularColor("Specular Color", Color) = (1,1,1)
    _SpecularRange ("Specular Range",  Range(0, 1)) = 0.9
        _SpecularMulti ("Specular Multi", Range(0, 1)) = 0.4
    _SpecularGloss("Sprecular Gloss", Range(0.001, 8)) = 4
        [Header(Fresnel)]
        [Space(20)]
        _RimColor ("_RimColor", Color)=(1,1,1,1)
        _RimMin ("_RimMin",Range(0,1)) = 0.5
        _RimMax ("_RimMax",Range(0,1)) = 0.5
        _RimSmooth ("_RimSmooth",Range(0,1)) = 0.5
        //_RimBloomExp("_RimBloomExp",Range(0,1)) = 0.5
        //_RimBloomMulti ("_RimBloomMulti ",Range(0,1)) = 0.5
        [Header(Outline)]
        [Space(20)]
        _OutlineWidth ("Outline Width", Range(0, 1)) = 0.24
        _OutlineColor ("OutLine Color", Color) = (0.5,0.5,0.5,1)

    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Tags{"LightMode"="UniversalForward"}
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
            TEXTURE2D(_RampTex);
            SAMPLER(sampler_RampTex);
            TEXTURE2D(_IlmTex);
            SAMPLER(sampler_IlmTex);

            float4 _MainTex_ST;
            float4 _MainColor;
            float4 _ShadowColor;
            float _ShadowRange;

            half3 _SpecularColor;
      half _SpecularRange;
          half _SpecularMulti;
      half _SpecularGloss;

            half4 _RimColor;
            float _RimMin;
            float _RimMax;
            float _RimSmooth;
            //float _RimBloomExp;
            //float _RimBloomMulti;

            v2f vert (a2v v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);                
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half3 worldNormal = normalize(i.worldNormal);
                half3 worldLightDir = normalize(_MainLightPosition.xyz);
                half3 worldviewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * _MainColor;
                //半兰伯特
                half halfLambert = dot(worldNormal, worldLightDir) * 0.5 + 0.5;
                 ////二分色阶
                //half3 diffuse = halfLambert > _ShadowRange ? _MainColor : _ShadowColor;
                ////二分色阶+平滑
                //half ramp = smoothstep(0,_ShadowRange, halfLambert -_ShadowColor);
                //用渐变图分色阶
                half ramp =  SAMPLE_TEXTURE2D(_RampTex,sampler_RampTex, float2(saturate(halfLambert - _ShadowRange), 0.5));
                //漫反射
                half3 diffuse = lerp(_ShadowColor, _MainColor, ramp);
                //高光
                half3 specular = 0;
                half4 ilmTex = SAMPLE_TEXTURE2D(_IlmTex,sampler_IlmTex,i.uv);
                half3 halfDir = normalize(worldLightDir + worldviewDir);
                half NdotH = max(0, dot(worldNormal, halfDir));
                half SpecularSize = pow(NdotH, _SpecularGloss);
                half specularMask = ilmTex.b;
                if (SpecularSize >= 1 - specularMask * _SpecularRange)
                {
                  specular = _SpecularMulti * (ilmTex.r) * _SpecularColor;
                }
                //边缘光
                half f =  1.0 - saturate(dot(worldviewDir, worldNormal));
                half rim = smoothstep(_RimMin, _RimMax, f);
                rim = smoothstep(0, _RimSmooth, rim);
                half3 rimColor = rim * _RimColor.rgb *  _RimColor.a;
                ////在《离岛之歌》中截图中，bloom的曝光主要集中在光照方向，边缘光的部分。这里对边缘光的公式做点小trick。将边缘光乘以漫反射公式，来获得比较符合光照方向边缘光。将它的值赋给Alpha通道。
                //half NdotL = max(0, dot(worldNormal, worldLightDir));
                //half rimBloom = pow (f, _RimBloomExp) * _RimBloomMulti * NdotL;
                //col.a = rimBloom;
                //环境光
                Light light = GetMainLight();
                half3 ambient = _GlossyEnvironmentColor.rgb * light.color.rgb;
                //混合
                col.rgb = col.rgb *  diffuse + specular + rimColor;
                //col.rgb = ilmTex.bbb;
                return col;
            }
            ENDHLSL
        }

        //描边
         Pass
        {
            Tags{ "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct a2v
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
            real4 _OutlineColor;
            real _OutlineWidth;
            CBUFFER_END

            v2f vert (a2v v)
            {

                v2f o;
                float4 pos =  TransformObjectToHClip(v.vertex.xyz);
                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal.xyz);
                float3 ndcNormal = normalize(TransformWViewToHClip(viewNormal.xyz)) * pos.w;//将法线变换到NDC空间
                float4 nearUpperRight = mul(unity_CameraInvProjection, float4(1, 1, UNITY_NEAR_CLIP_VALUE, _ProjectionParams.y));//将近裁剪面右上角位置的顶点变换到观察空间
                float aspect = abs(nearUpperRight.y / nearUpperRight.x);//求得屏幕宽高比
                ndcNormal.x *= aspect;
                pos.xy += 0.01 * _OutlineWidth * ndcNormal.xy;
                o.pos = pos;

                return o;
            }

            real4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
    FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}


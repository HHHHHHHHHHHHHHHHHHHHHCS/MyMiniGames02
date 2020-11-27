Shader "My/FFXVWarpStrike/FresnelGlow"
{
	Properties
	{
		[HDR]_GlowColor ("GlowColor", Color) = (0, 0.929374, 1.866066, 1)
		_GlowPower ("GlowPower", Float) = 1
		_AlphaThreshold ("AlphaThreshold", Float) = 0
		_FresnelAmount ("FresnelAmount", Vector) = (1, 1, 1, 1)
	}
	
	SubShader
	{
		Tags { "RenderType" = "LightweightPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
		
		Pass
		{
			Tags { "LightMode" = "LightweightForward" }
			
			Blend One One
			Cull Back
			ZTest LEqual
			ZWrite Off
			
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			// Unity defined keywords
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON
			
			
			// Lightweight Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#define _AlphaClip 1
			
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Shadows.hlsl"
			
			struct appdata
			{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
				float4 tangent: TANGENT;
				float2 uv0: TEXCOORD0;
				float2 uv1: TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 clipPos: SV_POSITION;
				float2 uv: TEXCOORD0;
				float3 wPos: TEXCOORD1;	//这里其实也可以把他放到 TBN 里面 变成float4
				float3 wNormal: TEXCOORD2;
				float3 wTangent: TEXCOORD3;
				float3 wBiTangent: TEXCOORD4;
				float3 wSpaceViewDirection: TEXCOORD5;
				float3 lightmapUV: TEXCOORD6;
				float3 fogFactorAndVertexLight: TEXCOORD7;
				float3 vertexSH: TEXCOORD8;
				#ifdef _MAIN_LIGHT_SHADOWS
					float4 shadowCoord: TEXCOORD9;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			CBUFFER_START(UnityPerMaterial)
			float4 _GlowColor;
			float _GlowPower;
			float _AlphaThreshold;
			float4 _FresnelAmount;
			CBUFFER_END
			
			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float3 worldSpacePosition = mul(UNITY_MATRIX_M, v.vertex).xyz;
				float3 worldSpaceNormal = TransformObjectToWorldNormal(v.normal, true);
				float3 worldSpaceTangent = normalize(mul((float3x3)UNITY_MATRIX_M, v.tangent.xyz));
				float3 worldSpaceBiTangent = cross(worldSpaceNormal, worldSpaceTangent.xyz) * v.tangent.w;
				float3 worldSpaceViewDirection = _WorldSpaceCamera.xyz - worldSpacePosition;
				
				o.uv = v.uv0;
				o.wPos = worldSpacePosition;
				o.wNormal = worldSpaceNormal;
				o.wTangent = worldSpaceTangent;
				o.wBiTangent = worldSpaceBiTangent;
				o.wSpaceViewDirection = worldSpaceViewDirection;
				o.clipPos = mul(UNITY_MATRIX_VP, float4(v.vertex, 1.0));
				
				OUTPUT_LIGHTMAP_UV(v.uv1, unity_LightmapST, o.lightmapUV);
				OUTPUT_SH(worldSpaceNormal, o.vertexSH);
				
				half3 vertexLight = VertexLighting(worldSpacePosition, worldSpaceNormal);
				half fogFactor = ComputeFogFactor(o.clipPos.z);
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#ifdef _MAIN_LIGHT_SHADOWS
					o.shadowCoord = TransformWorldToShadowCoord(worldSpacePosition);
				#endif
				
				return o;
			}
			
			
			void CalcSurfaceData(
				float3 WorldSpaceNormal, float3 worldSpaceViewDirection, float3 tangentSpaceNormal,
				out float3 Albedo, out float3 specular, out float metallic, out float3 normal, out float3 emission, out float smoothness,
				out float occlusion, out float occlusion, out float alpha, out float alphaClipThreshold)
			{
				
			}
			
			float4 frag(v2f i): SV_Target
			{
				i.wTangent = normalize(i.wTangent);
				i.wBiTangent = normalize(i.wBiTangent);
				i.wNormal = normalize(i.wNormal);
				i.wSpaceViewDirection = normalize(i.wSpaceViewDirection);
				
				float3x3 tangentSpaceTransform = float3x3(i.wTangent, i.wBiTangent, i.wNormal);
				float3 tangentSpaceNormal = mul(i.wNormal, (float3x3)tangentSpaceTransform).xyz;
				
				float3 albedo = float3(0.5, 0.5, 0.5);
				float3 specular = float3(0, 0, 0);
				float metallic = 1;
				float3 normal = float3(0, 0, 1);
				float3 emission = 0;
				float smoothness = 0.5;
				float occlusion = 1;
				float alpha = 1;
				float alphaClipThreshold = 0;
				
				
//TODO:


				#if _AlphaClip
					clip(Alpha - AlphaClipThreshold);
				#endif
				
				
				inputData.shadowCoord = IN.shadowCoord;
				inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, inputData.normalWS);
				
				color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
				
				
				
				return col;
			}
			ENDHLSL
			
		}
	}
}

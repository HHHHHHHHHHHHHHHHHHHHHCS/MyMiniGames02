Shader "My/FFXVWarpStrike/OpaqueGlow"
{
	Properties
	{
		[HDR]_GlowColor ("GlowColor", Color) = (0, 0.929374, 1.866066, 1)
		_BaseColor ("BaseColor", Color) = (0.5, 0.5, 0.5)
		_GlowPower ("GlowPower", Float) = 1
		_AlphaThreshold ("AlphaThreshold", Float) = 0
		_FresnelAmount ("FresnelAmount", Vector) = (1, 1, 1, 1)
	}
	
	SubShader
	{
		Tags { /* "RenderType" = "UniversalPipeline"*/ "RenderType" = "AlphaTest" "Queue" = "AlphaTest" }
		
		Pass
		{
			Name "UniversalForwardOnly"
			Tags { "LightMode" = "UniversalForwardOnly" }
			
			Blend One Zero
			Cull Back
			ZTest LEqual
			ZWrite On
			
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
			#define _OpaqueMode 1
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			
			#include "GlowPBR.hlsl"
			
			struct a2v
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
				float4 fogFactorAndVertexLight: TEXCOORD7;
				float3 vertexSH: TEXCOORD8;
				float4 shadowCoord: TEXCOORD9;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			v2f vert(a2v v)
			{
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float3 worldSpacePosition = mul(UNITY_MATRIX_M, v.vertex).xyz;
				float3 worldSpaceNormal = TransformObjectToWorldNormal(v.normal, true);
				float3 worldSpaceTangent = normalize(mul((float3x3)UNITY_MATRIX_M, v.tangent.xyz));
				float3 worldSpaceBiTangent = cross(worldSpaceNormal, worldSpaceTangent) * v.tangent.w;
				float3 worldSpaceViewDirection = _WorldSpaceCameraPos.xyz - worldSpacePosition;
				
				o.uv = v.uv0;
				o.wPos = worldSpacePosition;
				o.wNormal = worldSpaceNormal;
				o.wTangent = worldSpaceTangent;
				o.wBiTangent = worldSpaceBiTangent;
				o.wSpaceViewDirection = worldSpaceViewDirection;
				o.clipPos = TransformWorldToHClip(worldSpacePosition);
				
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
				
				CalcSurfaceData(i.wNormal, i.wSpaceViewDirection, tangentSpaceNormal, albedo, specular, metallic, normal, emission, smoothness, occlusion, alpha, alphaClipThreshold);
				
				#if _AlphaClip
					clip(alpha - alphaClipThreshold);
				#endif
				
				InputData inputData = (InputData)0;
				inputData.positionWS = i.wPos;
				inputData.normalWS = i.wNormal;
				inputData.shadowCoord = i.shadowCoord;
				inputData.fogCoord = i.fogFactorAndVertexLight.x;
				inputData.vertexLighting = i.fogFactorAndVertexLight.yzw;
				inputData.bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, inputData.normalWS);
				
				half4 color = UniversalFragmentPBR(inputData, albedo, metallic, specular,
				smoothness, occlusion, emission, alpha);
				
				color.rgb = MixFog(color.rgb, i.fogFactorAndVertexLight.x);
				
				return color;
			}
			ENDHLSL
			
		}
		
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			ZWrite On
			ZTest LEqual
			
			Cull Back
			
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			// Unity defined keywords
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			
			#define _AlphaClip 1
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			
			
			#include "GlowPBR.hlsl"
			
			struct a2v
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			float3 _LightDirection;
			
			v2f vert(a2v v)
			{
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float3 worldSpacePosition = mul(UNITY_MATRIX_M, v.vertex).xyz;
				float3 worldSpaceNormal = TransformObjectToWorldNormal(v.normal, true);
				float3 worldSpaceTangent = normalize(mul((float3x3)UNITY_MATRIX_M, v.tangent.xyz));
				float3 worldSpaceBiTangent = cross(worldSpaceNormal, worldSpaceTangent) * v.tangent.w;
				float3 worldSpaceViewDirection = _WorldSpaceCameraPos.xyz - worldSpacePosition;
				
				o.uv = v.uv0;
				o.wPos = worldSpacePosition;
				o.wNormal = worldSpaceNormal;
				o.wTangent = worldSpaceTangent;
				o.wBiTangent = worldSpaceBiTangent;
				o.wSpaceViewDirection = worldSpaceViewDirection;
				
				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(worldSpacePosition, worldSpaceNormal, _LightDirection));
				
				#if UNITY_REVERSED_Z
					positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#else
					positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#endif
				
				o.clipPos = positionCS;
				
				return o;
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
				
				CalcSurfaceData(i.wNormal, i.wSpaceViewDirection, tangentSpaceNormal, albedo, specular, metallic, normal, emission, smoothness, occlusion, alpha, alphaClipThreshold);
				
				#if _AlphaClip
					clip(alpha - alphaClipThreshold);
				#endif
				
				return 0;
			}
			
			ENDHLSL
			
		}
		
		Pass
		{
			Name "DepthOnly"
			Tags { "LightMode" = "DepthOnly" }
			
			ZWrite On
			ColorMask 0
			
			Cull Back
			
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile_instancing
			
			#define _AlphaClip 1
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			
			#include "GlowPBR.hlsl"
			
			struct a2v
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			v2f vert(a2v v)
			{
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float3 worldSpacePosition = mul(UNITY_MATRIX_M, v.vertex).xyz;
				float3 worldSpaceNormal = TransformObjectToWorldNormal(v.normal, true);
				float3 worldSpaceTangent = normalize(mul((float3x3)UNITY_MATRIX_M, v.tangent.xyz));
				float3 worldSpaceBiTangent = cross(worldSpaceNormal, worldSpaceTangent) * v.tangent.w;
				float3 worldSpaceViewDirection = _WorldSpaceCameraPos.xyz - worldSpacePosition;
				
				o.uv = v.uv0;
				o.wPos = worldSpacePosition;
				o.wNormal = worldSpaceNormal;
				o.wTangent = worldSpaceTangent;
				o.wBiTangent = worldSpaceBiTangent;
				o.wSpaceViewDirection = worldSpaceViewDirection;
				o.clipPos = TransformWorldToHClip(worldSpacePosition);
				
				return o;
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
				
				CalcSurfaceData(i.wNormal, i.wSpaceViewDirection, tangentSpaceNormal, albedo, specular, metallic, normal, emission, smoothness, occlusion, alpha, alphaClipThreshold);
				
				#if _AlphaClip
					clip(alpha - alphaClipThreshold);
				#endif
				
				
				return 0;
			}
			
			
			ENDHLSL
			
		}
		
		Pass
		{
			Name "Meta"
			Tags { "LightMode" = "Meta" }
			
			Cull Off
			
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile_instancing
			
			#define _AlphaClip 1
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			
			#include "GlowPBR.hlsl"
			
			struct a2v
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
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			v2f vert(a2v v)
			{
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				
				float3 worldSpacePosition = mul(UNITY_MATRIX_M, v.vertex).xyz;
				float3 worldSpaceNormal = TransformObjectToWorldNormal(v.normal, true);
				float3 worldSpaceTangent = normalize(mul((float3x3)UNITY_MATRIX_M, v.tangent.xyz));
				float3 worldSpaceBiTangent = cross(worldSpaceNormal, worldSpaceTangent) * v.tangent.w;
				float3 worldSpaceViewDirection = _WorldSpaceCameraPos.xyz - worldSpacePosition;
				
				o.uv = v.uv0;
				o.wPos = worldSpacePosition;
				o.wNormal = worldSpaceNormal;
				o.wTangent = worldSpaceTangent;
				o.wBiTangent = worldSpaceBiTangent;
				o.wSpaceViewDirection = worldSpaceViewDirection;
				o.clipPos = TransformWorldToHClip(worldSpacePosition);
				
				
				return o;
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
				
				CalcSurfaceData(i.wNormal, i.wSpaceViewDirection, tangentSpaceNormal, albedo, specular, metallic, normal, emission, smoothness, occlusion, alpha, alphaClipThreshold);
				
				#if _AlphaClip
					clip(alpha - alphaClipThreshold);
				#endif
				
				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = albedo;
				metaInput.Emission = emission;
				
				return MetaFragment(metaInput);
			}
			
			ENDHLSL
			
		}
	}
}

Shader "My/BoTWStasis/StasisObject"
{
	Properties
	{
		[HDR]_EmissionColor("Emission Color", Color) = (1.319508, 0, 0.04129754, 0)
		[HDR]_BorderColor("Border Color", Color) = (1, 1, 1, 0)
		_StasisAmount("Stasis Amount", Float) = 0.5
		[NoScaleOffset]_AlbedoTexture("AlbedoTexture", 2D) = "white" {}
		[NoScaleOffset]_NormalMap("NormalMap", 2D) = "white" {}
		_NoiseAmount("Noise Amount", Float) = 1
		_NoiseSpeed("NoiseSpeed", Float) = 0.5
		_NoiseWidth("NoiseWidth", Range(0, 0.1)) = 0.03
		_NoiseScale("NoiseScale", Float) = 20
	}
	HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
	CBUFFER_START(UnityPerMaterial)
	float4 _EmissionColor;
	float4 _BorderColor;
	float _StasisAmount;
	float _NoiseAmount;
	float _NoiseSpeed;
	float _NoiseWidth;
	float _NoiseScale;
	CBUFFER_END

	TEXTURE2D(_AlbedoTexture);
	SAMPLER(sampler_AlbedoTexture);
	TEXTURE2D(_NormalMap);
	SAMPLER(sampler_NormalMap);

	SurfaceData CalcSurfaceData()
	{
		SurfaceData o = (SurfaceData)0;
		return o;
	}
	ENDHLSL
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque" "Queue" = "Geometry"/* "RenderType" = "UniversalPipeline"*/
		}

		Pass
		{
			Name "UniversalForwardOnly"
			Tags
			{
				"LightMode" = "UniversalForwardOnly"
			}

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

			#pragma shader_feature _NORMALMAP

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"


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
				float3 wPos: TEXCOORD1; //这里其实也可以把他放到 TBN 里面 变成float4
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
				float3 worldSpaceBiTangent = cross(worldSpaceNormal, worldSpaceTangent) * v.tangent.w *
					unity_WorldTransformParams.w;
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
				//在fragment计算
				o.shadowCoord = 0;//TransformWorldToShadowCoord(worldSpacePosition);
				#endif

				return o;
			}

			float4 frag(v2f i): SV_Target
			{
				i.wTangent = normalize(i.wTangent);
				i.wBiTangent = normalize(i.wBiTangent);
				i.wNormal = normalize(i.wNormal);
				i.wSpaceViewDirection = SafeNormalize(i.wSpaceViewDirection);

				float3x3 tangentSpaceTransform = float3x3(i.wTangent, i.wBiTangent, i.wNormal);
				float3 tangentSpaceNormal = mul(i.wNormal, (float3x3)tangentSpaceTransform).xyz;

				SurfaceData surfaceData = CalcSurfaceData();

				InputData inputData = (InputData)0;
				inputData.positionWS = i.wPos;
				inputData.normalWS = i.wNormal;
				inputData.viewDirectionWS = i.wSpaceViewDirection;
				#if defined(MAIN_LIGHT_CALCULATE_SHADOWS)
				inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
				inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif
				// inputData.shadowCoord = i.shadowCoord;
				inputData.fogCoord = i.fogFactorAndVertexLight.x;
				inputData.vertexLighting = i.fogFactorAndVertexLight.yzw;
				inputData.bakedGI = SAMPLE_GI(i.lightmapUV, i.vertexSH, inputData.normalWS);

				half4 color = UniversalFragmentPBR(inputData, surfaceData);

				color.rgb = MixFog(color.rgb, i.fogFactorAndVertexLight.x);

				return color;
			}
			ENDHLSL

		}

		//copy by Package\com.unity.render-pipelines.universal@10.5.0\Shaders\ShadowCasterPass.hlsl
		Pass
		{
			Name "ShadowCaster"
			Tags
			{
				"LightMode" = "ShadowCaster"
			}

			ZWrite On
			ZTest LEqual
			Cull Back

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A


			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			float3 _LightDirection;

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
				float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
				float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

				#if UNITY_REVERSED_Z
				positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#else
				positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#endif

				return positionCS;
			}

			Varyings ShadowPassVertex(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);

				output.positionCS = GetShadowPositionHClip(input);
				return output;
			}

			half4 ShadowPassFragment(Varyings input) : SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}

		//copy by Package/com.unity.render-pipelines.universal@10.5.0/Shaders/DepthOnlyPass.hlsl
		Pass
		{
			Name "DepthOnly"
			Tags
			{
				"LightMode" = "DepthOnly"
			}

			ZWrite On
			ColorMask 0
			Cull Back

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			#pragma vertex DepthOnlyVertex
			#pragma fragment DepthOnlyFragment

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			//--------------------------------------
			// GPU Instancing
			#pragma multi_compile_instancing


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 position : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings DepthOnlyVertex(Attributes input)
			{
				Varyings output = (Varyings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.positionCS = TransformObjectToHClip(input.position.xyz);
				return output;
			}

			half4 DepthOnlyFragment(Varyings input) : SV_TARGET
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				return 0;
			}
			ENDHLSL
		}

		// This pass it not used during regular rendering, only for lightmap baking.
		//copy by Library/Package/com.unity.render-pipelines.universal@10.5.0/Shaders/LitMetaPass.hlsl
		Pass
		{
			Name "Meta"
			Tags
			{
				"LightMode" = "Meta"
			}

			Cull Off

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex UniversalVertexMeta
			#pragma fragment UniversalFragmentMeta

			#pragma shader_feature _SPECULAR_SETUP
			#pragma shader_feature _EMISSION
			#pragma shader_feature _METALLICSPECGLOSSMAP
			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			#pragma shader_feature _SPECGLOSSMAP


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				#ifdef _TANGENT_TO_WORLD
					float4 tangentOS     : TANGENT;
				#endif
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			Varyings UniversalVertexMeta(Attributes input)
			{
				Varyings output;
				output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2, unity_LightmapST,
				                                       unity_DynamicLightmapST);
				output.uv = input.uv0;
				return output;
			}

			half4 UniversalFragmentMeta(Varyings input) : SV_Target
			{
				SurfaceData surfaceData = CalcSurfaceData();

				BRDFData brdfData;
				InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular,
				                   surfaceData.smoothness, surfaceData.alpha, brdfData);

				MetaInput metaInput;
				metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
				metaInput.SpecularColor = surfaceData.specular;
				metaInput.Emission = surfaceData.emission;

				return MetaFragment(metaInput);
			}


			//LWRP -> Universal Backwards Compatibility
			Varyings LightweightVertexMeta(Attributes input)
			{
				return UniversalVertexMeta(input);
			}

			half4 LightweightFragmentMeta(Varyings input) : SV_Target
			{
				return UniversalFragmentMeta(input);
			}
			ENDHLSL
		}

		// unuse pass
		/*
		Pass
		{
			Name "Universal2D"
			Tags
			{
				"LightMode" = "Universal2D"
			}

			Blend One Zero
			ZWrite On
			Cull Back

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature _ALPHATEST_ON
			#pragma shader_feature _ALPHAPREMULTIPLY_ON

			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
			ENDHLSL
		}
		*/
	}
}
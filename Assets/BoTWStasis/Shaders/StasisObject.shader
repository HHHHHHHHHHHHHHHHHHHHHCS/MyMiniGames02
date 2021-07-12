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
	half4 _EmissionColor;
	half4 _BorderColor;
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

	inline float Unity_SawtoothWave_float(float In)
	{
		return 2 * (In - floor(0.5 + In));
	}

	inline float Unity_SimpleNoise_RandomValue_float(float2 uv)
	{
		return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
	}

	inline float Unity_SimpleNnoise_Interpolate_float(float a, float b, float t)
	{
		return (1.0 - t) * a + (t * b);
	}


	inline float Unity_SimpleNoise_ValueNoise_float(float2 uv)
	{
		float2 i = floor(uv);
		float2 f = frac(uv);
		f = f * f * (3.0 - 2.0 * f);

		// uv = abs(frac(uv) - 0.5);
		float2 c0 = i + float2(0.0, 0.0);
		float2 c1 = i + float2(1.0, 0.0);
		float2 c2 = i + float2(0.0, 1.0);
		float2 c3 = i + float2(1.0, 1.0);
		float r0 = Unity_SimpleNoise_RandomValue_float(c0);
		float r1 = Unity_SimpleNoise_RandomValue_float(c1);
		float r2 = Unity_SimpleNoise_RandomValue_float(c2);
		float r3 = Unity_SimpleNoise_RandomValue_float(c3);

		float bottomOfGrid = Unity_SimpleNnoise_Interpolate_float(r0, r1, f.x);
		float topOfGrid = Unity_SimpleNnoise_Interpolate_float(r2, r3, f.x);
		float t = Unity_SimpleNnoise_Interpolate_float(bottomOfGrid, topOfGrid, f.y);
		return t;
	}

	float Unity_SimpleNoise_float(float2 UV, float Scale)
	{
		float t = 0.0;

		float freq = pow(2.0, float(0));
		float amp = pow(0.5, float(3 - 0));
		t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

		freq = pow(2.0, float(1));
		amp = pow(0.5, float(3 - 1));
		t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

		freq = pow(2.0, float(2));
		amp = pow(0.5, float(3 - 2));
		t += Unity_SimpleNoise_ValueNoise_float(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

		return t;
	}

	float Unity_FresnelEffect_float(float3 Normal, float3 viewDir, float Power)
	{
		return pow((1.0 - saturate(dot(normalize(Normal), normalize(viewDir)))), Power);
	}

	float Unity_FresnelEffect_NoNormalize_float(float3 Normal, float3 viewDir, float Power)
	{
		return pow((1.0 - saturate(dot((Normal), (viewDir)))), Power);
	}

	SurfaceData CalcSurfaceData(float2 uv, float3 wNormal, float3 wView)
	{
		SurfaceData o = (SurfaceData)0;

		//albedo-------------
		half4 albedo = SAMPLE_TEXTURE2D(_AlbedoTexture, sampler_AlbedoTexture, uv);

		//normal-------------
		float4 normal = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv);
		real3 normalTS = UnpackNormal(normal);

		//noise effect--------------------
		float wave = Unity_SawtoothWave_float(_Time.y * _NoiseSpeed);
		float noise = Unity_SimpleNoise_float(uv, _NoiseScale);
		float n_a = wave - _NoiseWidth;
		float n_b = step(n_a, noise);
		float n_c = step(wave, noise);
		float n_d = n_b - n_c;

		//fresnel--------------
		float f_a = Unity_FresnelEffect_NoNormalize_float(wNormal, wView, 1);
		float f_b = Unity_FresnelEffect_NoNormalize_float(wNormal, wView, 7.77);
		half4 borderColor = f_b * _BorderColor;
		borderColor += _EmissionColor * f_a + _EmissionColor;
		borderColor *= _StasisAmount;
		half4 b_c = _EmissionColor * 3.32;
		b_c = lerp(1, b_c, 0.7);
		b_c *= n_d * _NoiseAmount;
		borderColor += b_c;

		o.albedo = albedo.rgb;
		o.normalTS = normalTS;
		o.emission = borderColor.rgb;
		o.metallic = 0.5;
		o.smoothness = 0;
		o.occlusion = 0.5;
		o.alpha = 1;
		o.clearCoatMask = 0.0;
		o.clearCoatSmoothness = 1.0;
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

			#pragma multi_compile _NORMALMAP
			#pragma multi_compile _NORMAL_DROPOFF_TS

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

			void BuildInputData(v2f input, SurfaceData surfaceData, out InputData inputData)
			{
				inputData.positionWS = input.wPos;

				#ifdef _NORMALMAP
				#if _NORMAL_DROPOFF_TS
				// IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
				half3x3 tangentSpaceTransform = half3x3(input.wTangent, input.wBiTangent, input.wNormal);
				inputData.normalWS = TransformTangentToWorld(surfaceData.normalTS, tangentSpaceTransform);
				#elif _NORMAL_DROPOFF_OS
			            inputData.normalWS = TransformObjectToWorldNormal(surfaceData.normalTS);
				#elif _NORMAL_DROPOFF_WS
			            inputData.normalWS = surfaceData.normalTS;
				#endif
				#else
			        inputData.normalWS = input.wNormal;
				#endif
				inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				inputData.viewDirectionWS = input.wSpaceViewDirection;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
			        inputData.shadowCoord = input.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
			        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
				#else
				inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif

				inputData.fogCoord = input.fogFactorAndVertexLight.x;
				inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
			}

			v2f vert(a2v v)
			{
				v2f o = (v2f)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 worldSpacePosition = mul(UNITY_MATRIX_M, v.vertex).xyz;
				float3 worldSpaceNormal = TransformObjectToWorldNormal(v.normal, true);
				float3 worldSpaceTangent = normalize(mul((float3x3)UNITY_MATRIX_M, v.tangent.xyz));
				float3 worldSpaceBiTangent = cross(worldSpaceNormal, worldSpaceTangent) * v.tangent.w *
					GetOddNegativeScale();
				float3 worldSpaceViewDirection = GetWorldSpaceViewDir(worldSpacePosition);

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

			float4 frag(v2f IN): SV_Target
			{
				IN.wTangent = normalize(IN.wTangent);
				IN.wBiTangent = normalize(IN.wBiTangent);
				IN.wNormal = normalize(IN.wNormal);
				IN.wSpaceViewDirection = SafeNormalize(IN.wSpaceViewDirection);

				SurfaceData surfaceData = CalcSurfaceData(IN.uv, IN.wNormal, IN.wSpaceViewDirection);

				InputData inputData = (InputData)0;
				BuildInputData(IN, surfaceData, inputData);

				half4 color = UniversalFragmentPBR(inputData, surfaceData);

				color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);

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
				float3 wNormal : TEXCOORD1;
				float3 wSpaceViewDirection : TEXCOORD2;
			};

			Varyings UniversalVertexMeta(Attributes input)
			{
				Varyings output;
				//因为是meta 所以 objectSpace 跟 worldSpace 没有什么区别
				output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.uv2, unity_LightmapST,
				                                       unity_DynamicLightmapST);
				output.uv = input.uv0;
				output.wNormal = TransformObjectToWorldNormal(input.normalOS);
				output.wSpaceViewDirection = GetWorldSpaceViewDir(input.positionOS.xyz);
				return output;
			}

			half4 UniversalFragmentMeta(Varyings IN) : SV_Target
			{
				IN.wNormal = normalize(IN.wNormal);
				IN.wSpaceViewDirection = SafeNormalize(IN.wSpaceViewDirection);

				SurfaceData surfaceData = CalcSurfaceData(IN.uv, IN.wNormal, IN.wSpaceViewDirection);

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
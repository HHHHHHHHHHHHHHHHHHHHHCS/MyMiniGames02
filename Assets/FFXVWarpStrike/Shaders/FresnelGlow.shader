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
			
			// #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			// #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			// #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			struct appdata
			{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
				float4 tangent: TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float2 uv: TEXCOORD0;
				float4 vertex: SV_POSITION;
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
				float3 worldSpaceNormal = normalize(mul(v.normal, (float3x3)UNITY_MATRIX_I_M));
				float3 worldSpaceTangent = normalize(mul((float3x3)UNITY_MATRIX_M, v.tangent.xyz));
				float3 worldSpaceBiTangent = cross(worldSpaceNormal, worldSpaceTangent.xyz) * v.tangent.w;
				float3 worldSpaceViewDirection = _WorldSpaceCamera.xyz - mul()
				
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				
				
				return o;
			}
			
			float4 frag(v2f i): SV_Target
			{
				
				return col;
			}
			ENDHLSL
			
		}
	}
}

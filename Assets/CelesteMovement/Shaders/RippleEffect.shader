Shader "My/CelesteMovement/RippleEffect"
{
	Properties
	{
	}
	SubShader
	{
		Lighting Off
		Cull Off
		ZTest Off
		ZWrite Off

		Pass
		{
			Name "Ripple Effect"

			HLSLPROGRAM
			#pragma vertexx vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct a2v
			{
				uint vertexID :SV_VertexID;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			TEXTURE2D(_MainTex);
			float2 _MainTex_TexelSize;

			TEXTURE2D(_GradTex);

			SAMPLER(sampler_Linear_Clamp);

			half4 _Reflection;
			float4 _Params1; // [ aspect, 1, scale, 0 ]
			float4 _Params2; // [ 1, 1/aspect, refraction, reflection ]

			float4 _Drop1;
			float4 _Drop2;
			float4 _Drop3;

			float Wave(float2 position, float2 origin, float time)
			{
				float d = length(position - origin);
				float t = time - d * _Params1.z;
				return (SAMPLE_TEXTURE2D(_GradTex, sampler_Linear_Clamp, float2(t,0)).r - 0.5) * 2;
			}


			float allwave(float2 position)
			{
				return
					Wave(position, _Drop1.xy, _Drop1.z) +
					Wave(position, _Drop2.xy, _Drop2.z) +
					Wave(position, _Drop3.xy, _Drop3.z);
			}


			v2f vert(a2v IN)
			{
				v2f o;
				o.vertex = GetFullScreenTriangleVertexPosition(IN.vertexID);
				o.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
				return o;
			}

			half4 frag(v2f IN):SV_Target
			{
			}
			ENDHLSL
		}
	}
}
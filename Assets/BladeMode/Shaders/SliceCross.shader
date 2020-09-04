Shader "My/BladeMode/SliceCross"
{
	Properties
	{
		[HDR]_Color ("Color", Color) = (0, 0, 0, 0)
		[HDR]_FresnelColor ("Fresnel Color", Color) = (0, 0, 0, 0)
		_FresnelIntensity ("Fresnel Intensity", float) = 4.5
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex: POSITION;
				float4 normal: NORMAL;
			};
			
			struct v2f
			{
				float4 vertex: SV_POSITION;
				float3 worldNormal: TEXCOORD0;
				float3 worldPos: TEXCOORD1;
			};
			
			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			float4 _FresnelColor;
			float _FresnelIntensity;
			CBUFFER_END
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			
			float4 frag(v2f i): SV_Target
			{
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
				float3 normal = normalize(i.worldNormal);
				float fre = saturate(1 - pow(dot(viewDir, normal), _FresnelIntensity));
				return lerp(_Color, _FresnelColor, fre);
			}
			ENDCG
			
		}
	}
}

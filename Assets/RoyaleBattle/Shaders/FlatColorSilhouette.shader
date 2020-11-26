﻿Shader "My/RoyaleBattle/FlatColorSilhouette"
{
	Properties
	{
		_Color ("Color", Color) = (1, 0, 0, 1)
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
			};
			
			struct v2f
			{
				float4 vertex: SV_POSITION;
			};
			
			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			CBUFFER_END
			
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			float4 frag(v2f i): SV_Target
			{
				return _Color;
			}
			ENDCG
			
		}
	}
}

Shader "My/RoyaleBattle/ForbiddenArea"
{
	Properties
	{
		_MainColor ("Main Color", Color) = (0.8113208, 0.3482556, 0.3482556, 0)
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "RendererQueue"="Transparent" }
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
				float2 uv: TEXCOORD0;
			};
			
			struct v2f
			{
				float2 uv: TEXCOORD0;
				float4 vertex: SV_POSITION;
			};
			
			float4 _MainColor;
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float4 frag(v2f i): SV_Target
			{
				float4 col = _MainColor;
				return col;
			}
			ENDCG
			
		}
	}
}

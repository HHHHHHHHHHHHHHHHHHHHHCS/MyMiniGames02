Shader "My/RoyaleBattle/ForbiddenArea"
{
	Properties
	{
		_MainColor ("Main Color", Color) = (0.8113208, 0.3482556, 0.3482556, 0)
	}
	SubShader
	{
		Tags { "RenderType" = "Transparent" "RendererQueue" = "Transparent" }
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

			CBUFFER_START(UnityPerMaterial)
			float4 _MainColor;
			CBUFFER_END
			
			
			float2 RotateDegrees(float2 uv, float2  center, float rotation)
			{
				rotation = rotation * (3.1415926f / 180.0f);
				uv -= center;
				float s = sin(rotation);
				float c = cos(rotation);
				
				float2x2 rMatrix = float2x2(c, -s, s, c);
				
				uv.xy = mul(uv.xy, rMatrix);
				uv += center;
				
				return uv;
			}
			
			float Remap(float input, float2 inMinMax, float2 outMinMax)
			{
				return outMinMax.x + (input - inMinMax.x) * (outMinMax.y - outMinMax.x) / (inMinMax.y - inMinMax.x);
			}
			
			float Rectangle(float2 uv, float width, float height)
			{
				float2 d = abs(uv * 2 - 1) - float2(width, height);
				d = 1 - d / fwidth(d);
				// 同上 fwidth  即ddx/ddy 的距离
				//w.x = abs(dFdx(uv).x);
				//w.y = abs(dFdy(uv).y);
				return saturate(min(d.x, d.y));
			}
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float4 frag(v2f i): SV_Target
			{
				float2 uv = i.uv;
				uv = RotateDegrees(uv, float2(0.5, 0.5), 45);
				
				float t = frac(_Time.x);
				
				uv += t;
				
				float r = uv.r * 30;
				r = frac(r);
				r = Remap(r, float2(0, 1), float2(0.1, 0.6));
				r = step(0.5, r);
				
				float d = Rectangle(i.uv, 0.97, 0.97);
				d = 1 - d;
				
				r = max(r, d);
				
				clip(r - 0.5);
				
				return float4(r * _MainColor.rgb, 1);
			}
			ENDCG
			
		}
	}
}

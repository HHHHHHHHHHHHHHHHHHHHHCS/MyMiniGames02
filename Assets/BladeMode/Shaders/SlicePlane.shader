Shader "Unlit/SlicePlane"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" { }
		[HDR]_Color ("Color", Color) = (0, 1, 1, 1)
		_FresnelIntensity ("Fresnel Intensity", float) = 4.5
		_DepthOffset ("Depth Offset", Range(0, 3)) = 0.3
		_DepthSoftness ("Depth Softness", Range(0, 1)) = 0.1
		[HDR]_SliceColor ("Slice Color", Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		LOD 100
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off
		
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
				float2 uv: TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex: SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 scrPos: TEXCOORD1;
				float hideAlpha: TEXCOORD2;
			};
			
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _FresnelIntensity;
			float _DepthOffset;
			float _DepthSoftness;
			float4 _SliceColor;
			
			sampler2D _CameraDepthTexture;
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.scrPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.scrPos.z);
				o.hideAlpha = v.uv.y;
				return o;
			}
			
			float4 frag(v2f i): SV_Target
			{
				
				float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));
				float isSlice = depth - i.scrPos.z - _DepthOffset;
				isSlice = smoothstep(_DepthSoftness, -_DepthSoftness, isSlice);
				float4 col = tex2D(_MainTex, i.uv) * _Color;
				col += isSlice * _SliceColor;
				float hideAlpha = 1 - abs(i.hideAlpha - 0.5);
				hideAlpha = smoothstep(0.5, 0.75, hideAlpha);
				col.a *= i.hideAlpha ;
				return col;
			}
			ENDCG
			
		}
	}
}

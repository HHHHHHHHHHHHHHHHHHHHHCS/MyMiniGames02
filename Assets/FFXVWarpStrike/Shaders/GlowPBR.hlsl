#ifndef __GLOWPBR_INCLUDE__
	#define __GLOWPBR_INCLUDE__
	
	CBUFFER_START(UnityPerMaterial)
	#ifdef _OpaqueMode
		float4 _BaseColor;
	#endif
	float4 _GlowColor;
	float _GlowPower;
	float _AlphaThreshold;
	float4 _FresnelAmount;
	CBUFFER_END
	
	void CalcSurfaceData(
		float3 worldSpaceNormal, float3 worldSpaceViewDirection, float3 tangentSpaceNormal,
		out float3 albedo, out float3 specular, out float metallic, out float3 normal, out float3 emission, out float smoothness,
		out float occlusion, out float alpha, out float alphaClipThreshold)
	{
		
		float fre = pow(1.0 - saturate(dot(worldSpaceNormal, worldSpaceViewDirection)), _GlowPower);
		float4 col = fre * _GlowColor * _FresnelAmount;
		
		#ifdef _OpaqueMode
			albedo = _BaseColor.rgb;
		#else
			albedo = col.rgb;
		#endif
		specular = float3(0, 0, 0);
		normal = tangentSpaceNormal;
		emission = col.rgb;
		metallic = 0;
		smoothness = 0.5;
		occlusion = 1;
		#ifdef _OpaqueMode
			alpha = 1;
			alphaClipThreshold = 0;
		#else
			alpha = fre;
			alphaClipThreshold = _AlphaThreshold;
		#endif
	}
	
#endif

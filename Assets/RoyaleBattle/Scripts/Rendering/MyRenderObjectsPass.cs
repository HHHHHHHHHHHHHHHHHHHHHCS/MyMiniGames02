using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static RoyaleBattle.MyRenderObjectsFeature;

namespace RoyaleBattle
{
	public class MyRenderObjectsPass : ScriptableRenderPass
	{
		private RenderQueueType renderQueueType;
		private FilteringSettings filteringSettings;
		private CustomCameraSettings cameraSettings;
		private string profilerTag;
		private ProfilingSampler profilingSampler;

		private List<ShaderTagId> shaderTagIdList = new List<ShaderTagId>();

		public Material overrideMaterial { get; set; }
		public int overrideMaterialPassIndex { get; set; }


		public MyRenderObjectsPass(RenderObjectsSettings settings)
		{
			var filterSettings = settings.filterSettings;

			profilerTag = settings.passTag;
			profilingSampler = new ProfilingSampler(profilerTag);
			renderPassEvent = settings.renderPassEvent;
			renderQueueType = filterSettings.renderQueueType;
			overrideMaterial = null;
			overrideMaterialPassIndex = 0;
			RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
				? RenderQueueRange.transparent
				: RenderQueueRange.opaque;
			filteringSettings = new FilteringSettings(renderQueueRange, filterSettings.layerMask,
				filterSettings.renderingLayerMask);

			var shaderTags = filterSettings.shaderTags;
			if (shaderTags != null && shaderTags.Length > 0)
			{
				foreach (var tag in shaderTags)
				{
					shaderTagIdList.Add(new ShaderTagId(tag));
				}
			}
			else
			{
				shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
				shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
				shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
			}
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
		}
	}
}
﻿using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RoyaleBattle
{
	[ExcludeFromPreset]
	public class MyRenderObjectsFeature : ScriptableRendererFeature
	{
		[System.Serializable]
		public enum RenderQueueType
		{
			Opaque,
			Transparent,
		}

		[System.Serializable]
		public class CustomCameraSettings
		{
			public bool overrideCamera = false;

			public bool restoreCamera = true;

			public Vector4 offset;

			public float cameraFieldOfView = 60.0f;
		}

		[System.Serializable]
		public class FilterSettings
		{
			public RenderQueueType renderQueueType;

			public LayerMask layerMask;

			//layer1->1 2->2 3->4 4->8 5->16 n->2^(n-1)
			public uint renderingLayerMask;

			public string[] shaderTags;

			public FilterSettings()
			{
				renderQueueType = RenderQueueType.Opaque;
				layerMask = 0;
				renderingLayerMask = 0;
			}
		}

		[System.Serializable]
		public class RenderObjectsSettings
		{
			public string passTag = "RenderObjectsFeature";

			public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

			public FilterSettings filterSettings = new FilterSettings();

			public Material overrideMaterial = null;

			public int overrideMaterialPassIndex = 0;

			public bool overrideDepthState = false;

			public CompareFunction depthCompareFunction = CompareFunction.LessEqual;

			public bool enableWrite = true;

			public StencilStateData stencilSettings = new StencilStateData();

			public CustomCameraSettings cameraSettings = new CustomCameraSettings();
		}

		public RenderObjectsSettings settings = new RenderObjectsSettings();

		private MyRenderObjectsPass renderObjectsPass;

		public override void Create()
		{
			renderObjectsPass = new MyRenderObjectsPass(settings);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			renderer.EnqueuePass(renderObjectsPass);
		}
	}
}
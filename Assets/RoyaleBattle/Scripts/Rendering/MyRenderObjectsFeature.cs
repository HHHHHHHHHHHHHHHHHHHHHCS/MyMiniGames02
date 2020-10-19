using UnityEngine;
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

			public uint renderingLayerMask;
			
			public string[] passNames;

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

			public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

			public FilteringSettings filterSettings = new FilteringSettings();

			public Material overrideMaterial = null;

			public int overrideMaterialPassIndex = 0;

			public bool overrideDepthState = false;

			public CompareFunction depthCompareFunction = CompareFunction.LessEqual;

			public bool enableWrite = true;

			public StencilStateData stencilSettings = new StencilStateData();

			public CustomCameraSettings cameraSettings = new CustomCameraSettings();
		}
		
		//TODO:layer1->1 2->2 3->4 4->8 5->16

		public override void Create()
		{
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
		}
	}
}
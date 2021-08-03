using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RippleRenderPassFeature : ScriptableRendererFeature
{
	class RippleRenderPass : ScriptableRenderPass
	{
		// This method is called before executing the render pass.
		// It can be used to configure render targets and their clear state. Also to create temporary render target textures.
		// When empty this render pass will render to the active camera render target.
		// You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
		// The render pipeline will ensure target setup and clearing happens in a performant manner.
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
		}

		// Here you can implement the rendering logic.
		// Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
		// https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
		// You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
		}

		// Cleanup any allocated resources that were created during the execution of this render pass.
		public override void OnCameraCleanup(CommandBuffer cmd)
		{
		}
	}

	public Shader rippleShader;

	private RippleRenderPass scriptablePass;

	private Material rippleMaterial;

	public override void Create()
	{
		if (rippleMaterial != null)
		{
			rippleMaterial = CoreUtils.CreateEngineMaterial(rippleShader);
		}
		
		//todo:

		scriptablePass = new RippleRenderPass
		{
			renderPassEvent = RenderPassEvent.BeforeRenderingPrepasses
		};
	}

	private void OnDestroy()
	{
		CoreUtils.Destroy(rippleMaterial);
	}

	// Here you can inject one or multiple render passes in the renderer.
	// This method is called when setting up the renderer once per-camera.
	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(scriptablePass);
	}
}
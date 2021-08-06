using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CelesteMovement.Scripts
{
	public class RippleRenderPassFeature : ScriptableRendererFeature
	{
		private class RippleRenderPass : ScriptableRenderPass
		{
			private const string k_tag = "RippleEffect";

			private static int tempRT_ID = Shader.PropertyToID("_TempRT");

			private static RenderTargetIdentifier cameraColorTexture_RTI =
				new RenderTargetIdentifier("_CameraColorTexture");

			private static RenderTargetIdentifier tempRT_RTI = new RenderTargetIdentifier(tempRT_ID);

			private static int srcTex_ID = Shader.PropertyToID("_SrcTex");

			public RippleRenderPass()
			{
				profilingSampler = new ProfilingSampler(k_tag);
			}

			// This method is called before executing the render pass.
			// It can be used to configure render targets and their clear state. Also to create temporary render target textures.
			// When empty this render pass will render to the active camera render target.
			// You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
			// The render pipeline will ensure target setup and clearing happens in a performant manner.
			// public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
			// {
			// }

			// Here you can implement the rendering logic.
			// Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
			// https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
			// You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				RippleEffect rippleEffect = RippleEffect.instance;

				if (rippleEffect == null || !rippleEffect.HaveEffect)
				{
					return;
				}

				CommandBuffer cmd = CommandBufferPool.Get(k_tag);
				using (new ProfilingScope(cmd, profilingSampler))
				{
					//blit
					cmd.GetTemporaryRT(tempRT_ID, renderingData.cameraData.cameraTargetDescriptor);
					cmd.SetRenderTarget(tempRT_RTI);
					cmd.SetGlobalTexture(srcTex_ID, cameraColorTexture_RTI);
					CoreUtils.DrawFullScreen(cmd, rippleEffect.RippleMat, null, 1);

					context.ExecuteCommandBuffer(cmd);
					cmd.Clear();

					cmd.SetRenderTarget(cameraColorTexture_RTI);
					cmd.SetGlobalTexture(srcTex_ID, tempRT_RTI);
					CoreUtils.DrawFullScreen(cmd, rippleEffect.RippleMat, null, 0);
					cmd.ReleaseTemporaryRT(tempRT_ID);
				}

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}

			// Cleanup any allocated resources that were created during the execution of this render pass.
			// public override void OnCameraCleanup(CommandBuffer cmd)
			// {
			// }
		}


		private RippleRenderPass scriptablePass;


		public override void Create()
		{
			scriptablePass = new RippleRenderPass
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
			};
		}

		// Here you can inject one or multiple render passes in the renderer.
		// This method is called when setting up the renderer once per-camera.
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (RippleEffect.instance != null)
			{
				renderer.EnqueuePass(scriptablePass);
			}
		}
	}
}
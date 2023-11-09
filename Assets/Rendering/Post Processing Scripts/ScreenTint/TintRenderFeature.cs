using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TintRenderFeature : ScriptableRendererFeature
{
    private TintPass tintPass;
    public override void Create()
    {
        tintPass = new TintPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(tintPass);
    }

    class TintPass : ScriptableRenderPass
    {
        private Material _mat;
        int tintId = Shader.PropertyToID("_Temp");
        RenderTargetIdentifier src, tint;

        public TintPass()
        {
            if (!_mat)
            {
                _mat = CoreUtils.CreateEngineMaterial("CustomPost/ScreenTint");
            }
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            src = renderingData.cameraData.renderer.cameraColorTargetHandle;
            cmd.GetTemporaryRT(tintId, desc, FilterMode.Bilinear);
            tint = new RenderTargetIdentifier(tintId);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer commandBuffer = CommandBufferPool.Get("TintRenderFeature");
            VolumeStack volumes = VolumeManager.instance.stack;
            ScreenTint screenTint = volumes.GetComponent<ScreenTint>();

            if (screenTint.IsActive())
            {
                _mat.SetColor("_OverlayColor", (Color)screenTint.tintColor);
                _mat.SetFloat("_Intensity", (float)screenTint.tintIntensity);

                Blit(commandBuffer, src, tint, _mat, 0);
                Blit(commandBuffer, tint, src);
            }

            context.ExecuteCommandBuffer(commandBuffer);
            CommandBufferPool.Release(commandBuffer);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tintId);
        }
    }
}

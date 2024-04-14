using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomVignetteRenderFeature : ScriptableRendererFeature
{
    private CustomVignettePass tintPass;

    public override void Create()
    {
        tintPass = new CustomVignettePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(tintPass);
    }

    class CustomVignettePass : ScriptableRenderPass
    {
        private Material _mat;
        int tintId = Shader.PropertyToID("_Temp");
        RenderTargetIdentifier src, tint;

        public CustomVignettePass()
        {
            if (!_mat)
            {
                _mat = CoreUtils.CreateEngineMaterial("CustomPost/CustomVignette");
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
            CustomVignette customVignette = volumes.GetComponent<CustomVignette>();

            if (customVignette.IsActive())
            {
                _mat.SetFloat(HashReference._radiusProperty, (float)customVignette.radius);
                _mat.SetFloat(HashReference._featherProperty, (float)customVignette.feather);
                _mat.SetColor(HashReference._mainColorProperty, (Color)customVignette.color);
                _mat.SetTexture(HashReference._imageTexProperty, (Texture)customVignette.vignetteTexture);

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

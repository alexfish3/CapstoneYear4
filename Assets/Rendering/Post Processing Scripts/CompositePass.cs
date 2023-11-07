using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CompositePass : ScriptableRenderPass
{
    private CompositeFeature.CustomPassSettings settings;

    private RenderTargetIdentifier colorBuffer, pixelBuffer;
    private int pixelBufferID = Shader.PropertyToID("_PixelBuffer");

    private Material material;
    private Material workingMaterial;
    //private RenderTexture renderTexture;
    private Camera renderCam;

    public RenderTexture texture1;
    public RenderTexture texture2;

    public CompositePass(CompositeFeature.CustomPassSettings settings)
    {
        this.settings = settings;
        this.material = settings.material;

        this.texture1 = settings.texture1;
        this.texture2 = settings.texture2;

        //this.renderTexture = settings.renderTexture;
        this.renderPassEvent = settings.renderPassEvent;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        workingMaterial = new Material(material);
        workingMaterial.SetTexture("_Texture1", texture1);
        workingMaterial.SetTexture("_Texture2", texture2);

        renderCam = renderingData.cameraData.camera;

        //if (renderTexture == null || renderTexture.width != Screen.width || renderTexture.height != Screen.height)
        //{
        //    renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        //    renderCam.targetTexture = renderTexture;
        //}

        colorBuffer = renderingData.cameraData.renderer.cameraColorTarget;
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;

        //pixelScreenHeight = settings.screenHeight;
        //pixelScreenWidth = (int)(pixelScreenHeight * renderingData.cameraData.camera.aspect + 0.5f);

        //material.SetVector("_BlockCount", new Vector2(pixelScreenWidth, pixelScreenHeight));
        //material.SetVector("_BlockSize", new Vector2(1.0f / pixelScreenWidth, 1.0f / pixelScreenHeight));
        //material.SetVector("_HalfBlockSize", new Vector2(0.5f / pixelScreenWidth, 0.5f / pixelScreenHeight));

        //descriptor.height = pixelScreenHeight;
        //descriptor.width = pixelScreenWidth;

        cmd.GetTemporaryRT(pixelBufferID, descriptor, FilterMode.Point);
        pixelBuffer = new RenderTargetIdentifier(pixelBufferID);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, new ProfilingSampler("Composite Pass")))
        {
            // No-shader variant
            //Blit(cmd, colorBuffer, pointBuffer);
            //Blit(cmd, pointBuffer, pixelBuffer);
            //Blit(cmd, pixelBuffer, colorBuffer);

            Blit(cmd, colorBuffer, pixelBuffer, material);
            Blit(cmd, pixelBuffer, colorBuffer);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (cmd == null) throw new System.ArgumentNullException("cmd");
        cmd.ReleaseTemporaryRT(pixelBufferID);
        //cmd.ReleaseTemporaryRT(pointBufferID);
    }

}
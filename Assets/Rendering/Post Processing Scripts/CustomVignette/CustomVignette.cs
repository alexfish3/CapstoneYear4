using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Custom Vignette", typeof(UniversalRenderPipeline))]
public class CustomVignette : VolumeComponent, IPostProcessComponent
{
    public FloatParameter radius = new FloatParameter(1);
    public FloatParameter feather = new FloatParameter(1);
    public ColorParameter color = new ColorParameter(Color.black);
    public TextureParameter vignetteTexture = new TextureParameter(null);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return true;
    }
}

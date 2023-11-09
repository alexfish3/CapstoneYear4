using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Screen Tint", typeof(UniversalRenderPipeline))]
public class ScreenTint : VolumeComponent, IPostProcessComponent
{
    public FloatParameter tintIntensity = new FloatParameter(1);
    public ColorParameter tintColor = new ColorParameter(Color.white);

    public bool IsActive()
    {
        return true;
    }

    public bool IsTileCompatible()
    {
        return true;
    }
}

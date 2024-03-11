using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Numerics;

public class PhaseIndicator : MonoBehaviour
{
    bool initalized = false;

    [SerializeField] Slider hornSlider;
    [SerializeField] float intensity = 3;

    [Range(0f, 2f)]
    [SerializeField] float hornGlowValue = 0;
    public float hornValueMax;
    float hornGlowStep;

    [SerializeField] Color readyColor;
    [SerializeField] Gradient hornGlowGraident;
  
    [Header("Material Info")]
    [SerializeField] Renderer ghostRenderer;
    [SerializeField] Material hornGlowRef;
    [SerializeField] Material hornGlow;

    private SoundPool soundPool; // for playing SFX
    private bool dirtyBoostReady = true;

    public bool ShowPhase = false;

    Coroutine hornStatus;

    // Define a delegate for the completion of the glow depletion
    public delegate void GlowDepleteComplete();

    private void Start()
    {
        soundPool = GetComponent<SoundPool>();

        hornGlowStep = hornValueMax / 100;
    }

    /// <summary>
    /// Sets the reference to the horn glow, is called during during customization to correctly reference horns with player materials
    /// </summary>
    public void ReferenceHornMaterial()
    {
        hornGlow = new Material(hornGlowRef);

        float factor = Mathf.Pow(2, (1 + intensity));

        Color color = new Color(readyColor.r * factor, readyColor.g * factor, readyColor.b * factor);

        hornGlow.SetColor("_BaseColor", readyColor);
        hornGlow.SetColor("_EmissionColor", color);

        Material[] ghostMaterials = ghostRenderer.materials;
        ghostMaterials[1] = hornGlow;

        ghostRenderer.materials = ghostMaterials;

        initalized = true;
    }

    /// <summary>
    /// Sets the horn color value
    /// </summary>
    public void SetHornColor(float passIn)
    {
        hornSlider.value = passIn;
        hornGlowValue = passIn;

        float intensityFactor = Mathf.Pow(2, (hornGlowValue + intensity));

        // Ready boost color
        if (passIn >= 1f)
        {
            if (!dirtyBoostReady)
            {
                soundPool.PlayBoostReady();
                dirtyBoostReady = true;
            }

            Color color = new Color(readyColor.r * intensityFactor, readyColor.g * intensityFactor, readyColor.b * intensityFactor);
            hornGlow.SetColor("_EmissionColor", color);
            hornGlow.SetColor("_BaseColor", readyColor);

        }
        else
        {
            dirtyBoostReady = false;
            Color currentColor = (hornGlowGraident.Evaluate(hornGlowValue / hornValueMax));
            Color color = new Color(currentColor.r * intensityFactor, currentColor.g * intensityFactor, currentColor.b * intensityFactor);
            hornGlow.SetColor("_EmissionColor", color);
            hornGlow.SetColor("_BaseColor", currentColor);
        }
    }

}

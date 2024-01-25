using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class PhaseIndicator : MonoBehaviour
{
    bool initalized = false;

    [SerializeField] Slider hornSlider;
    [SerializeField] float intensity = 3;

    [Range(0f, 2f)]
    [SerializeField] float hornGlowValue = 0;
    public float hornValueMax;

    [SerializeField] Color readyColor;
    [SerializeField] Gradient hornGlowGraident;
  
    [Header("Material Info")]
    [SerializeField] Renderer ghostRenderer;
    [SerializeField] Material hornGlowRef;
    [SerializeField] Material hornGlow;

    [Header("Phase Camera")]
    [SerializeField] UniversalAdditionalCameraData mainCameraData;

    private SoundPool soundPool; // for playing SFX
    private bool dirtyBoostReady = true;

    public bool ShowPhase = false;

    private void Start()
    {
        soundPool = GetComponent<SoundPool>();
    }

    // Update is called once per frame
    void Update()
    {
        if (initalized == false)
            return;

        float factor = Mathf.Pow(2, (hornGlowValue + intensity));

        // Ready boost color
        if (hornGlowValue >= hornValueMax - 0.01f)
        {
            if(!dirtyBoostReady)
            {
                soundPool.PlayBoostReady();
                dirtyBoostReady = true;
            }
            Color color = new Color(readyColor.r * factor, readyColor.g * factor, readyColor.b * factor);
            hornGlow.SetColor("_MainColor", color);
        }
        else
        {
            dirtyBoostReady = false;
            Color currentColor = (hornGlowGraident.Evaluate(hornGlowValue / hornValueMax));
            Color color = new Color(currentColor.r * factor, currentColor.g * factor, currentColor.b * factor);
            hornGlow.SetColor("_MainColor", color);
        }
    }

    /// <summary>
    /// Sets the reference to the horn glow, is called during during customization to correctly reference horns with player materials
    /// </summary>
    public void ReferenceHornMaterial()
    {
        hornGlow = new Material(hornGlowRef);

        Material[] ghostMaterials = ghostRenderer.materials;

        ghostMaterials[1] = hornGlow;

        ghostRenderer.materials = ghostMaterials;

        initalized = true;
    }

    /// <summary>
    /// Sets the horn glow value
    /// </summary>
    public void SetHornGlow(float newValue)
    {
        hornGlowValue = newValue;
    }

    /// <summary>
    /// Begins the horn glow charge
    /// </summary>
    public IEnumerator BeginHornGlow(float cooldown)
    {
        hornSlider.value = 0f;

        float hornGlowStep = hornValueMax / 100;

        for (int i = 0; i < 100; i++)
        {
            float progress = i / 100f;  // Calculate the progress from 0 to 1
            hornSlider.value = Mathf.Lerp(0f, 1f, progress);  // Set the slider value based on the progress

            hornGlowValue += hornGlowStep;
            yield return new WaitForSeconds(cooldown / 100);
        }
    }

    /// <summary>
    /// Sets the phase cam to be either phase or normal based on bool input
    /// </summary>
    public void SetPhaseCam(bool enabled)
    {
        if (ShowPhase == false)
            return;

        // Phase Cam View
        if (enabled)
        {
            mainCameraData.SetRenderer(2);
        }
        // Normal Cam View
        else
        {
            mainCameraData.SetRenderer(1);
        }
    }
}

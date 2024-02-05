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

    [Header("Phase Camera")]
    [SerializeField] LayerMask drivingMask;
    [SerializeField] LayerMask phasingMask;
    [SerializeField] UniversalAdditionalCameraData mainCameraData;

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

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.B) && hornSlider.value < 1f)
        {
            hornGlowValue = hornValueMax + 1;
            hornSlider.value = 1;
            StopAllCoroutines();
        }
        if (initalized == false)
            return;
    }

    /// <summary>
    /// Sets the reference to the horn glow, is called during during customization to correctly reference horns with player materials
    /// </summary>
    public void ReferenceHornMaterial()
    {
        hornGlow = new Material(hornGlowRef);

        float factor = Mathf.Pow(2, (1 + intensity));
        Color color = new Color(readyColor.r * factor, readyColor.g * factor, readyColor.b * factor);
        hornGlow.SetColor("_MainColor", color);

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

    public void BeginGlowCharge(float cooldown)
    {
        if (hornStatus != null)
            StopCoroutine(hornStatus);

        hornStatus = StartCoroutine(GlowCharge(cooldown));
    }

    public void BeginGlowDepleate(float cooldown)
    {
        if (hornStatus != null)
            StopCoroutine(hornStatus);

        hornStatus = StartCoroutine(GlowDeplete(cooldown));
    }

    /// <summary>
    /// Begins the horn glow charge
    /// </summary>
    public IEnumerator GlowCharge(float cooldown)
    {
        hornSlider.value = 0f;

        float waitTime = cooldown / 100f;

        for (int i = 0; i < 101; i++)
        {
            hornSlider.value = i / 100f;  // Set the slider value based on the progress

            hornGlowValue += hornGlowStep;

            float factor = Mathf.Pow(2, (hornGlowValue + intensity));

            // Ready boost color
            if (hornGlowValue >= hornValueMax - 0.01f)
            {
                if (!dirtyBoostReady)
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

            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Begins the horn glow depleate
    /// </summary>
    public IEnumerator GlowDeplete(float cooldown, GlowDepleteComplete onComplete = null)
    {
        float waitTime = cooldown / 100f;

        for (int i = 100; i > -1; i--)
        {
            hornSlider.value = i / 100f;

            hornGlowValue -= hornGlowStep;

            float factor = Mathf.Pow(2, (hornGlowValue + intensity));

            Color currentColor = (hornGlowGraident.Evaluate(hornGlowValue / hornValueMax));
            Color color = new Color(currentColor.r * factor, currentColor.g * factor, currentColor.b * factor);
            hornGlow.SetColor("_MainColor", color);

            yield return new WaitForSeconds(waitTime);
        }

        Debug.Log("Glow depletion complete");

        // Invoke the completion callback if provided
        onComplete?.Invoke();
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

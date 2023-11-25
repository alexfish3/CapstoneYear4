using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class PhaseIndicator : MonoBehaviour
{

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

    // Start is called before the first frame update
    void Start()
    {
        hornGlow = new Material(hornGlowRef);

        Material[] ghostMaterials = ghostRenderer.materials;

        ghostMaterials[1] = hornGlow;

        ghostRenderer.materials = ghostMaterials;
    }

    // Update is called once per frame
    void Update()
    {
        float factor = Mathf.Pow(2, (hornGlowValue + intensity));

        // Ready boost color
        if (hornGlowValue >= hornValueMax - 0.01f)
        {
            Color color = new Color(readyColor.r * factor, readyColor.g * factor, readyColor.b * factor);
            hornGlow.SetColor("_MainColor", color);
        }
        else
        {
            Color currentColor = (hornGlowGraident.Evaluate(hornGlowValue / hornValueMax));
            Color color = new Color(currentColor.r * factor, currentColor.g * factor, currentColor.b * factor);
            hornGlow.SetColor("_MainColor", color);
        }


    }

    public void SetHornGlow(float newValue)
    {
        hornGlowValue = newValue;
    }

    public IEnumerator beginHornGlow(float cooldown)
    {
        float hornGlowStep = hornValueMax / 100;

        for (int i = 0; i < 100; i++)
        {
            hornGlowValue += hornGlowStep;
            yield return new WaitForSeconds(cooldown / 100);
        }
    }
}

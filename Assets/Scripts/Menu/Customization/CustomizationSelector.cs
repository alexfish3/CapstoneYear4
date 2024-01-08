using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationSelector : MonoBehaviour
{
    const int CUSTOMIZATION_OPTIONS = 2;

    [Header("Character Customization Options")]
    [SerializeField] CustomizationChanging customizationChanging;

    [SerializeField] int currentPlayerColor = 0;
    [SerializeField] Material[] PlayerColors;

    [SerializeField] int currentPlayerHat = 0;
    [SerializeField] Mesh[] PlayerHats;

    [Header("References")]
    [SerializeField] GameObject selector;
    [SerializeField] MeshRenderer ghostModel;
    public Material[] ghostMats;

    [SerializeField] GameObject[] sliderGameobjcts;

    Animator currentSlider;
    [SerializeField] Animator colorSlider;
    [SerializeField] CustomizationSlider colorSliderCustomization;

    [SerializeField] Animator hatSlider;
    [SerializeField] CustomizationSlider hatSliderCustomization;

    [SerializeField] PhaseIndicator phaseIndicator;

    enum CustomizationChanging
    {
        playerColor = 0,
        playerHat = 1
    }

    public void Start()
    {
        UpdatePlayerVisual();
    }

    public void SetCustomizationType(bool direction)
    {
        // Positive Scroll
        if (direction)
        {
            if ((int)customizationChanging == CUSTOMIZATION_OPTIONS - 1)
            {
                customizationChanging = 0;
            }
            else
            {
                customizationChanging = customizationChanging + 1;
            }
        }
        // Negative Scroll
        else
        {
            if ((int)customizationChanging == 0)
            {
                customizationChanging = (CustomizationChanging)CUSTOMIZATION_OPTIONS - 1;
            }
            else
            {
                customizationChanging = customizationChanging - 1;
            }
        }

        // Updates selector for current slider selected
        selector.transform.position = sliderGameobjcts[(int)customizationChanging].transform.position;
    }

    public void SetCustomizationValue(bool direction)
    {
        switch (customizationChanging)
        {
            case CustomizationChanging.playerColor:
                currentSlider = colorSlider;
                SetIntValue(direction, ref currentPlayerColor, PlayerColors.Length);
                UpdatePlayerVisual();
                return;
            case CustomizationChanging.playerHat:
                currentSlider = hatSlider;
                SetIntValue(direction, ref currentPlayerHat, PlayerHats.Length);
                return;
        }
    }

    public void SetIntValue(bool direction, ref int valueToChange, int maxAmount)
    {
        // Positive Scroll
        if (direction)
        {
            if (valueToChange == maxAmount - 1)
            {
                valueToChange = 0;
            }
            else
            {
                valueToChange++;
            }

            colorSliderCustomization.currentMainPos = valueToChange;
            currentSlider.SetTrigger("SlideLeft");
            //UpdateIcons(colorSliderCustomization, colorSprites, valueToChange);

        }
        // Negative Scroll
        else
        {
            if (valueToChange == 0)
            {
                valueToChange = maxAmount - 1;
            }
            else
            {
                valueToChange--;
            }

            colorSliderCustomization.currentMainPos = valueToChange;
            currentSlider.SetTrigger("SlideRight");

            //UpdateIcons(colorSliderCustomization, colorSprites, valueToChange);
        }

    }

    public void UpdatePlayerVisual()
    {
        Debug.Log("UpdatePlayerVisual() called");
        ghostMats = ghostModel.materials;
        ghostMats[0] = PlayerColors[currentPlayerColor];
        ghostModel.materials = ghostMats;

        phaseIndicator.ReferenceHornMaterial();
    }
}

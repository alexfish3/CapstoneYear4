using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HornColor : MonoBehaviour
{
    [SerializeField] Color charging, full;
    [SerializeField] Slider slider;
    [SerializeField] Image sliderImage;

    private void Start()
    {
        slider.onValueChanged.AddListener(CheckValue);
        sliderImage.color = new Color(255,0,235);
    }

    private void CheckValue(float value)
    {
        if(value == 1)
        {
            sliderImage.color = new Color(255,0,235);
        }
        else
        {
            sliderImage.color = new Color(255, 255, 255);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the headlight on the scooter.
/// </summary>
public class HeadlightController : MonoBehaviour
{
    [SerializeField] private Light headlight;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapStartingCutscene += EnableHeadlight;
        GameManager.Instance.OnSwapResults += DisableHeadlight;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapStartingCutscene -= EnableHeadlight;
        GameManager.Instance.OnSwapResults -= DisableHeadlight;
    }

    private void EnableHeadlight()
    {
        if(headlight != null)
            headlight.enabled = true;
    }

    private void DisableHeadlight() // no head?
    {
        if(headlight != null)
            headlight.enabled = false;
    }
}

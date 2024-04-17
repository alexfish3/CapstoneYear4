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
        GameManager.Instance.OnSwapStartingCutscene += () => headlight.enabled = true;
        GameManager.Instance.OnSwapResults += () => headlight.enabled = false;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapStartingCutscene -= () => headlight.enabled = true;
        GameManager.Instance.OnSwapResults -= () => headlight.enabled = false;
    }
}

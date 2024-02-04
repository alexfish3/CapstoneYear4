using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for the trail icons used on the heatmap. Destroys them when the game state changes.
/// </summary>
public class HeatmapTrailIcon : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.Instance.OnSwapStartingCutscene += DestroyIcon;
        GameManager.Instance.OnSwapGoldenCutscene += DestroyIcon;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapStartingCutscene -= DestroyIcon;
        GameManager.Instance.OnSwapGoldenCutscene -= DestroyIcon;
    }

    private void DestroyIcon()
    {
        Destroy(this.gameObject);
    }
}

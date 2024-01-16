using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject tint;
    
    public enum PauseType
    {
        Host,
        Sub
    }
    public PauseType pauseType = PauseType.Sub;

    [Header("Sub Pause")]
    [SerializeField] GameObject subPauseGO;

    [Header("Host Pause")]
    [SerializeField] GameObject hostPauseGO;

    public void OnPause()
    {
        tint.SetActive(true);

        switch(pauseType)
        {
            case PauseType.Host:
                hostPauseGO.SetActive(true);
                return;
            case PauseType.Sub:
                subPauseGO.SetActive(true);
                return;
            default: return;
        }

    }

    public void OnPlay()
    {
        hostPauseGO.SetActive(false);
        subPauseGO.SetActive(false);

        tint.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSwapper : MonoBehaviour
{
    //[SerializeField] private GameObject mainMap;
    [SerializeField] private GameObject finalMap;


    private void OnEnable()
    {
        GameManager.Instance.OnSwapStartingCutscene += SwapToMain;
        GameManager.Instance.OnSwapGoldenCutscene += SwapToFinal;
    }
    
    private void OnDisable()
    {
        GameManager.Instance.OnSwapStartingCutscene -= SwapToMain;
        GameManager.Instance.OnSwapStartingCutscene -= SwapToFinal;
    }

    private void SwapToMain()
    {
        ////mainMap.SetActive(true); 
        //finalMap.SetActive(false);
    }
    public void SwapToFinal()
    {
        ////mainMap.SetActive(false);
        //finalMap.SetActive(true);
    }
}

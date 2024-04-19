using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Special functionality for the final order. I'm amazed this script isn't being written earlier.
/// </summary>
public class FinalOrder : MonoBehaviour
{
    [SerializeField] private Order finalOrder;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapAnything += EraseFinal;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnSwapAnything -= EraseFinal;
    }
    void Start()
    {
        finalOrder.InitOrder();
    }

    private void EraseFinal()
    {
        if(GameManager.Instance.MainState != GameState.GoldenCutscene && GameManager.Instance.MainState != GameState.FinalPackage && finalOrder.IsActive)
        {
            finalOrder.EraseGoldWithoutDelivering();
        }
    }
}

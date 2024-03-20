using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Special functionality for the final order. I'm amazed this script isn't being written earlier.
/// </summary>
public class FinalOrder : MonoBehaviour
{
    private Order finalOrder;

    private void Awake()
    {
        finalOrder = GetComponent<Order>();
    }

    void Start()
    {
        OrderManager.Instance.AddOrder(finalOrder);
        GameManager.Instance.SetGameState(GameState.GoldenCutscene);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Special functionality for the final order. I'm amazed this script isn't being written earlier.
/// </summary>
public class FinalOrder : MonoBehaviour
{
    [SerializeField] private Order finalOrder;

    void Start()
    {
        finalOrder.InitOrder();
        //finalOrder.InitOrder();
        //GameManager.Instance.SetGameState(GameState.GoldenCutscene);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for the cardboard cutout in the tutorial.
/// </summary>
public class CutoutHandler : OrderHandler
{
    [Tooltip("Order the cutout will be holding.")]
    [SerializeField] private Order order;

    [Tooltip("Position the order will be.")]
    [SerializeField] private Transform orderPos;
    // Start is called before the first frame update
    void Start()
    {
        order.CardboardHold(this);
    }
}

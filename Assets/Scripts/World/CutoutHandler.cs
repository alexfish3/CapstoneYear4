using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for the cardboard cutout in the tutorial.
/// </summary>
public class CutoutHandler : MonoBehaviour
{
    [Tooltip("Order the cutout will be holding.")]
    [SerializeField] private Order order;

    [Tooltip("Position the order will be.")]
    [SerializeField] private Transform orderPos;

    private bool hasStolen = false;
    // Start is called before the first frame update
    public void InitCutout()
    {
        order.CardboardHold();
        order.transform.position = orderPos.position;
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasStolen)
            return;

        OrderHandler player;
        BallDriving playerBall;

        try
        {
            player = other.gameObject.transform.parent.GetComponentInChildren<OrderHandler>();
            playerBall = other.gameObject.transform.parent.GetComponentInChildren<BallDriving>();
            if (playerBall.Boosting)
            {
                order.StealActive = false;
                order.InitOrder(false);
                player.AddOrder(order);
                hasStolen = true;
            }
        }
        catch
        {
            return;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// This class is meant to be attached to every player and will handle picking up, dropping, stealing, and delivering orders in the game, as well as player score.
/// </summary>
public class OrderHandler : MonoBehaviour
{
    private int score; // score of the player
    public int Score { get { return score; } }
    private Order order1; // first order the player is holding
    private Order order2; // second order the player is holding

    [Tooltip("Positions the orders will snap to on the back of the scooter")]
    [SerializeField] private Transform order1Position;
    [SerializeField] private Transform order2Position;

    private void Start()
    {
        score = 0; // init score to 0
    }

    /// <summary>
    /// This method checks if the player can carry an order and adds it to their bike if they can.
    /// </summary>
    /// <param name="inOrder">Order the player is trying to pick up</param>
    public void AddOrder(Order inOrder)
    {
        // will add order if it fits, elsewise will not do anything
        if(order1 == null || order2 == null)
        {
            if(order2 == null)
            {
                order2 = inOrder;
                order2.transform.position = order2Position.position;
            }
            else
            {
                order1 = inOrder;
                order1.transform.position = order1Position.position;
            }
            inOrder.PickupOrder();
            inOrder.transform.parent = this.transform;
        }
    }

    /// <summary>
    /// This method is called when the player delivers an order to the dropoff location. If the player has the right order it will drop it off and award the player.
    /// </summary>
    /// <param name="rightOrder">The correct order of the dropoff spot</param>
    public void DeliverOrder(Order rightOrder)
    {
        if(order1 == rightOrder)
        {
            score += (int)order1.Value;
            order1.Dropoff();
            order1 = null;
        }
        else if(order2 == rightOrder)
        {
            score += (int)order2.Value;
            order2.Dropoff();
            order2 = null;
        }
    }
}

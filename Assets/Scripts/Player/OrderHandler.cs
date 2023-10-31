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
    [Tooltip("Exposed for testing purposes only!")]
    [SerializeField] private Order order1; // first order the player is holding
    [SerializeField] private Order order2; // second order the player is holding

    [Tooltip("Positions the orders will snap to on the back of the scooter")]
    [SerializeField] private Transform order1Position;
    [SerializeField] private Transform order2Position;

    [Tooltip("Determines whether the player is boosting or not. Exposed for testing purposes only.")]
    [SerializeField] private bool isBoosting;
    public bool IsBoosting { get { return IsBoosting; } }

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
        if (inOrder.CanPickup)
        {
            // will add order if it fits, elsewise will not do anything
            if (order1 == null || order2 == null)
            {
                if (order2 == null)
                {
                    order2 = inOrder;
                    order2.transform.position = order2Position.position;
                }
                else
                {
                    order1 = inOrder;
                    order1.transform.position = order1Position.position;
                }
                inOrder.Pickup(this.gameObject);
                inOrder.transform.parent = this.transform;
            }
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
            order1.Deliver();
            order1 = null;
        }
        else if(order2 == rightOrder)
        {
            score += (int)order2.Value;
            order2.Deliver();
            order2 = null;
        }
    }

    /// <summary>
    /// This method is for when the player "spins" out. It will drop all the orders the player is currently holding.
    /// </summary>
    public void DropEverything()
    {
        if(order1 != null)
        {
            order1.Drop();
            order1 = null;
        }
        if(order2 != null)
        {
            order2.Drop();
            order2 = null;
        }
    }

    /// <summary>
    /// This method returns the order of the highest value the player is holding. If the player isn't holidng anything it returns null.
    /// </summary>
    /// <returns></returns>
    public Order GetBestOrder()
    {
        if(order1 != null && order2 != null)
        {
            if(order2.Value > order1.Value)
            {
                return order2;
            }
            else
            {
                return order1;
            }
        }
        else if(order1 != null)
        {
            return order1;
        }
        else if(order2 != null)
        {
            return order2;
        }
        else
        {
            return null;
        }

    }

    /// <summary>
    /// This method accepts an order as a parameter and checks if the player is holding it. If they are it will remove it from their possession. Doesn't "drop" the order, intended for stealing.
    /// </summary>
    /// <param name="inOrder">Order to be removed</param>
    public void LoseOrder(Order inOrder)
    {
        if(inOrder == order1)
        {
            order1 = null;
        }
        else if(inOrder == order2)
        {
            order2 = null;
        }
    }

    /// <summary>
    /// This method steals the best order from a victim player. It will then make the victim drop their other order if they have one.
    /// </summary>
    /// <param name="victimPlayer">Player being stolen from</param>
    void StealOrder(OrderHandler victimPlayer)
    {
        Order newOrder = victimPlayer.GetBestOrder();
        if (newOrder != null)
        {
            AddOrder(newOrder);
            victimPlayer.LoseOrder(newOrder);
        }
        victimPlayer.DropEverything();
    }

    /// <summary>
    /// Typical onCollisionEnter. Handles stealing orders from other players.
    /// </summary>
    /// <param name="other">Collision player has hit. Will attempt to steal if this hitbox is another player</param>
    private void OnCollisionEnter(Collision other)
    {
        OrderHandler otherHandler;
        try
        {
            otherHandler = other.gameObject.GetComponent<OrderHandler>();
        }
        catch
        {
            otherHandler = null;
        }
        if (otherHandler != null && isBoosting && !otherHandler.isBoosting)
        {
            StealOrder(otherHandler);
            isBoosting = false;
        }
    }
}

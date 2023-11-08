using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Manages the scores of all players and assigns placements.
/// </summary>
public class ScoreManager : SingletonMonobehaviour<ScoreManager>
{
    private List<OrderHandler> orderHandlers = new List<OrderHandler>(); // list of order handlers in the scene

    /// <summary>
    /// Adds an OrderHandler to the list if they're not already in the list.
    /// </summary>
    /// <param name="inOrderHandler">OrderHandler to be added</param>
    public void AddOrderHandler(OrderHandler inOrderHandler)
    {
        if(!orderHandlers.Contains(inOrderHandler))
        {
            orderHandlers.Add(inOrderHandler);
            UpdatePlacement();
        }
    }

    /// <summary>
    /// Sorts the orderHandlers list based on their scores and assigns them placements.
    /// Called when a new OrderHandler is added to the list and when a delivery is made.
    /// </summary>
    public void UpdatePlacement()
    {
        orderHandlers.Sort((i, j) => j.Score.CompareTo(i.Score));
        for(int i=0; i<orderHandlers.Count; i++)
        {
            if(i>0)
            {
                if (orderHandlers[i].Score == orderHandlers[i-1].Score) // checks if score is the same with previous OH, basically allows for ties
                {
                    orderHandlers[i].Placement = orderHandlers[i-1].Placement;
                    return;
                }
            }
            orderHandlers[i].Placement = i + 1;
        }
    }
}

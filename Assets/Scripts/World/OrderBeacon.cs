using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// This class is for the beacon to indicate where an order is and where it needs to be delivered to
/// </summary>
public class OrderBeacon : MonoBehaviour
{
    private Order order; // order the beacon is tracking
    private bool isPickup = true;
    public bool IsPickup { get { return isPickup; } }

    private MeshRenderer meshRenderer; // renderer to change the color
    private Color color;

    private CompassMarker compassMarker; // for the dropoff location on the compass marker
    public CompassMarker CompassMarker { get { return compassMarker; } }
    private void Awake()
    {
        meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
        compassMarker = this.gameObject.GetComponent<CompassMarker>();
    }

    /// <summary>
    /// This method initializes the beacon. It sets the order the beacon is tracking, sets the color, and the position.
    /// </summary>
    /// <param name="inOrder">Order the beacon will track</param>
    public void InitBeacon(Order inOrder)
    {
        this.transform.parent = OrderManager.Instance.transform;
        order = inOrder;

        switch(order.Value)
        {
            case Order.Order_Value.Easy:
                color = new Color(0,1,0,0.5f);
                break;
            case Order.Order_Value.Medium:
                color = new Color(1,1,0,0.5f);
                break;
            case Order.Order_Value.Hard:
                color = new Color(1,0,0,0.5f);
                break;
            default:
                break;
        }

        meshRenderer.material.color = color;
        this.transform.position = order.transform.position;
    }

    /// <summary>
    /// This method changes the pickup beacon to a dropoff beacon.
    /// </summary>
    /// <param name="dropoff">Dropoff location</param>
    public void SetDropoff(Transform dropoff)
    {
        this.transform.position = dropoff.position;
        meshRenderer.material.color = new Color(1,1,1,0.5f);
        isPickup = false;
        order.PlayerHolding.GetComponent<Compass>().AddCompassMarker(compassMarker);
    }

    /// <summary>
    /// This method will set the beacon back to a pickup beacon in the event it is knocked out of the player's possession.
    /// </summary>
    public void ResetPickup()
    {
        this.transform.position = order.transform.position;
        meshRenderer.material.color = color;
        isPickup = true;
        order.PlayerHolding.GetComponent<Compass>().RemoveCompassMarker(compassMarker);
    }

    /// <summary>
    /// This method erases the beacon and removes its dropoff marker from the player's UI.
    /// </summary>
    public void EraseBeacon()
    {
        order.PlayerHolding.GetComponent<Compass>().RemoveCompassMarker(compassMarker);
        Destroy(gameObject);
    }
    /// <summary>
    /// Basic OnTriggerEnter, will execute whenever something enters the beacon's light.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<OrderHandler>() != null)
        {
            if (isPickup)
            {
                other.GetComponent<OrderHandler>().AddOrder(order); // add the order if the beacon is a pickup beacon
            }
            else
            {
                other.GetComponent<OrderHandler>().DeliverOrder(order); // deliver the order if the beacon is a dropoff beacon
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for the order items in the gaame. It has simple methods for adding and dropping off orders, and contains the enum for the values of the orders.
/// </summary>
public class Order : MonoBehaviour
{
    /// <summary>
    /// Value of an order.
    /// </summary>
    public enum Order_Value
    {
        Easy = 10,
        Medium = 20,
        Hard = 50
    }

    private Order_Value value;
    public Order_Value Value { get { return value; } }
    private Transform pickup;
    private Transform dropoff;

    [Tooltip("Reference to the beacon prefab this class creates")]
    [SerializeField] private GameObject beaconPrefab;
    private OrderBeacon beacon;

    /// <summary>
    /// This method initializes the order with passed in values and sets its default location. It also initializes the beacon for this order.
    /// </summary>
    /// <param name="inPickup">Order's pickup point</param>
    /// <param name="inDropoff">Order's dropoff point</param>
    /// <param name="inValue">Value of the order</param>
    public void InitOrder(Transform inPickup, Transform inDropoff, Order_Value inValue)
    {
        pickup = inPickup;
        dropoff = inDropoff;
        value = inValue;

        this.transform.position = pickup.position;

        beaconPrefab = Instantiate(beaconPrefab);
        beacon = beaconPrefab.GetComponent<OrderBeacon>();
        beacon.InitBeacon(this);
    }
    
    /// <summary>
    /// This method is called when the order is picked up by a player.
    /// </summary>
    public void PickupOrder()
    {
        beacon.SetDropoff(dropoff);
    }

    /// <summary>
    /// This method is called when the player successfully delivers an order.
    /// </summary>
    public void Dropoff()
    {
        Destroy(beaconPrefab);
        EraseOrder();
    }

    /// <summary>
    /// This method erases the order and adds its pickup and dropoff points back to the list in OrderManager.
    /// </summary>
    private void EraseOrder()
    {
        OrderManager.Instance.AddPickupDropoff(pickup, dropoff);
        OrderManager.Instance.IncrementCounters(value, -1);
        OrderManager.Instance.RemoveOrder(this);
        Destroy(this.gameObject);
    }
}

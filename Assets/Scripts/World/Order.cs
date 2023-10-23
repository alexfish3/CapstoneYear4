using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order : MonoBehaviour
{
    public enum Order_Value
    {
        Easy = 10,
        Medium = 20,
        Hard = 50
    }

    private Order_Value value;
    private Transform pickup;
    private Transform dropoff;
    public void InitOrder(Transform inPickup, Transform inDropoff, Order_Value inValue)
    {
        pickup = inPickup;
        dropoff = inDropoff;
        value = inValue;

        this.transform.position = dropoff.position;
    }

    public void EraseOrder()
    {
        OrderManager.Instance.AddPickupDropoff(pickup, dropoff);
        Destroy(this);
    }
}

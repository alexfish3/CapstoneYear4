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
        Hard = 50,
        Golden = 100
    }

    private Order_Value value;
    public Order_Value Value { get { return value; } }
    private MeshRenderer meshRenderer;
    private Transform pickup;
    private Transform dropoff;

    private OrderHandler playerHolding = null;
    public OrderHandler PlayerHolding { get {  return playerHolding; } }

    [Tooltip("Time between a player dropping a package and being able to pick it back up again")]
    [SerializeField] private int pickupCooldown = 3;
    private bool canPickup = true; // if a player can pickup this order
    public bool CanPickup { get { return canPickup; } }

    [Tooltip("Reference to the beacon prefab this class creates")]
    [SerializeField] private GameObject beaconPrefab;
    private OrderBeacon beacon;

    [Tooltip("Reference to the compass marker component on this object")]
    [SerializeField] CompassMarker compassMarker;
    [SerializeField] Sprite[] possiblePackageTypes;

    private IEnumerator pickupCooldownCoroutine; // IEnumerator reference for pickupCooldown coroutine
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
        
        // setting the color of the order
        meshRenderer = GetComponent<MeshRenderer>();
        switch (value)
        {
            case Order_Value.Easy:
                meshRenderer.material.color = Color.green;
                compassMarker.icon = possiblePackageTypes[0];
                break;
            case Order_Value.Medium:
                meshRenderer.material.color = Color.yellow;
                compassMarker.icon = possiblePackageTypes[1];
                break;
            case Order_Value.Hard:
                meshRenderer.material.color = Color.red;
                compassMarker.icon = possiblePackageTypes[2];
                break;
            case Order_Value.Golden:
                meshRenderer.material.color = Color.black;
                compassMarker.icon = possiblePackageTypes[3];
                break;
            default:
                break;
        }

        compassMarker.InitalizeCompassUIOnAllPlayers();
    }
    
    /// <summary>
    /// This method is called when the order is picked up by a player.
    /// </summary>
    public void Pickup(OrderHandler player)
    {
        playerHolding = player;
        beacon.SetDropoff(dropoff);
        
        compassMarker.SwitchCompassUIForPlayers(true);
    }

    /// <summary>
    /// This method drops an order at its current location.
    /// </summary>
    public void Drop()
    {
        beacon.ResetPickup();
        playerHolding = null;
        this.transform.parent = OrderManager.Instance.transform;
        StartPickupCooldownCoroutine();
    }

    /// <summary>
    /// This method is called when the player successfully delivers an order.
    /// </summary>
    public void Deliver()
    {
        beacon.EraseBeacon();
        playerHolding = null;
        EraseOrder();
        // Removes the ui from all players
        compassMarker.RemoveCompassUIFromAllPlayers();
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

    /// <summary>
    /// This method removes the beacon compass marker from the UI of playerHolding then sets playerHolding to null.
    /// </summary>
    public void RemovePlayerHolding()
    {
        if (playerHolding != null)
        {
            playerHolding.GetComponent<Compass>().RemoveCompassMarker(beacon.CompassMarker);
        }
        playerHolding = null;
    }

    private void StartPickupCooldownCoroutine()
    {
        if(pickupCooldownCoroutine == null)
        {
            pickupCooldownCoroutine = PickupCooldown();
            StartCoroutine(pickupCooldownCoroutine);
        }
    }

    private void StopPickupCountdownCoroutine()
    {
        if(pickupCooldownCoroutine != null)
        {
            StopCoroutine(pickupCooldownCoroutine);
            pickupCooldownCoroutine = null;
        }
    }

    /// <summary>
    /// Coroutine that runs every time an order is knocked out of a player's possession. Will make the order ungrabbable for a predetermined time.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PickupCooldown()
    {
        canPickup = false;
        yield return new WaitForSeconds(pickupCooldown);
        canPickup = true;
        StopPickupCountdownCoroutine();
    }
}

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
        Easy = 20,
        Medium = 40,
        Hard = 60,
        Golden = 69
    }

    private Order_Value value;
    public Order_Value Value { get { return value; } }
    private MeshRenderer meshRenderer;
    private Transform pickup;
    private Transform dropoff;
    private Transform lastGrounded;
    public Transform LastGrounded { get { return lastGrounded; } set { lastGrounded = value;} }
    private OrderHandler playerHolding = null;
    public OrderHandler PlayerHolding { get {  return playerHolding; } }
    private OrderHandler playerDropped; // for cooldown with losing an order
    public OrderHandler PlayerDropped { get {  return playerDropped; } }

    [Tooltip("Time between a player dropping a package and being able to pick it back up again")]
    [SerializeField] private int pickupCooldown = 3;
    private bool canPickup = true; // if a player can pickup this order
    public bool CanPickup { get { return canPickup; } }
    [Tooltip("Default height of the order")]
    [SerializeField] private float height = 4.43f;

    [Tooltip("Reference to the beacon prefab this class creates")]
    [SerializeField] private GameObject beaconPrefab;
    private OrderBeacon beacon;

    [Tooltip("Reference to the compass marker component on this object")]
    [SerializeField] CompassMarker compassMarker;
    [SerializeField] Sprite[] possiblePackageTypes;

    [Tooltip("The hdr color options for the different tiers of packages")]
    [SerializeField][ColorUsageAttribute(true, true)]public Color[] packageColors;

    [Tooltip("Layermasks for respawn logic. Should be set to building phase checker, water (ignore raycast), and ground")]
    [SerializeField] LayerMask water,ground,buildingCheck;

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
        
        // setting the color of the order
        meshRenderer = GetComponent<MeshRenderer>();

        // Determines the pacakge type value based on the order type
        int packageType = 0;
        switch (value)
        {
            case Order_Value.Easy:
                packageType = 0;
                break;
            case Order_Value.Medium:
                packageType = 1;
                break;
            case Order_Value.Hard:
                packageType = 2;
                break;
            case Order_Value.Golden:
                packageType = 3;
                break;
        }

        meshRenderer.material.color = packageColors[packageType];

        Color beaconColor = new Color(packageColors[packageType].r, packageColors[packageType].g, packageColors[packageType].b, 0.3f);
        beacon.InitBeacon(this, beaconColor);
        compassMarker.icon = possiblePackageTypes[packageType];

        compassMarker.InitalizeCompassUIOnAllPlayers();
    }
    
    /// <summary>
    /// This method is called when the order is picked up by a player.
    /// </summary>
    public void Pickup(OrderHandler player)
    {
        playerHolding = player;
        if(value == Order_Value.Golden)
        {
            playerHolding.HasGoldenOrder = true;
        }
        playerDropped = null;
        StopPickupCountdownCoroutine();
        beacon.SetDropoff(dropoff);
        
        compassMarker.SwitchCompassUIForPlayers(true);
    }

    /// <summary>
    /// This method drops an order at its current location.
    /// </summary>
    public void Drop(Vector3 newPosition)
    {
        RaycastHit hit;
        bool foundSpot = false;
        transform.LookAt(Vector3.zero);
        //transform.rotation = Quaternion.Euler(0,transform.rotation.y,0);
        // adjusts position if necessary
        for (int i = 0; i < 10; i++)
        {
            if(Physics.Raycast(newPosition, Vector3.down, out hit, Mathf.Infinity, ground) && Physics.Raycast(newPosition, Vector3.down, out hit, Mathf.Infinity, water)
                && !Physics.Raycast(newPosition, Vector3.down, out hit, Mathf.Infinity, buildingCheck))
            {
                foundSpot = true;
                break;
            }
            else
            {
                newPosition += transform.forward;
            }
        }
        if (!foundSpot) // spawns at pickup if couldn't find another spot
        {
            newPosition = pickup.position;
        }
        transform.position = newPosition;
        this.transform.parent = OrderManager.Instance.transform;
        beacon.ResetPickup();
        if(value == Order_Value.Golden)
        {
            playerHolding.HasGoldenOrder = false;
        }
        playerDropped = playerHolding;
        playerHolding = null;
        StartPickupCooldownCoroutine();
    }
    /// <summary>
    /// This method erases the order and adds its pickup and dropoff points back to the list in OrderManager.
    /// </summary>
    public void EraseOrder()
    {
        // Removes the ui from all players
        compassMarker.RemoveCompassUIFromAllPlayers();
        beacon.EraseBeacon();
        OrderManager.Instance.IncrementCounters(value, -1);
        OrderManager.Instance.RemoveOrder(this);
        if(value == Order_Value.Golden)
        {
            OrderManager.Instance.GoldOrderDelivered(); // lets the OM know the golden order has been delivered
            playerHolding.Score += OrderManager.Instance.FinalOrderValue - (int)Order.Order_Value.Golden;
            playerHolding.HasGoldenOrder = false;
        }
        else
        {
            OrderManager.Instance.AddPickupDropoff(pickup, dropoff);
        }
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    public void EraseGoldWithoutDelivering()
    {
        // Removes the ui from all players
        if (playerHolding != null)
        {
            playerHolding.HasGoldenOrder = false;
            playerHolding = null;
        }
        compassMarker.RemoveCompassUIFromAllPlayers();
        beacon.EraseBeacon();
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
        playerDropped = null;
        StopPickupCountdownCoroutine();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for the order items in the game. It has simple methods for adding and dropping off orders, and contains the enum for the values of the orders.
/// </summary>
public class Order : MonoBehaviour
{

    [SerializeField] private Constants.OrderValue value;
    public Constants.OrderValue Value { get { return value; } }

    [SerializeField] private bool isActive = false;
    public bool IsActive { get { return isActive; } set { isActive = value; } }
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;

    [SerializeField] private Transform pickup;
    [SerializeField] private Transform dropoff;
    private Transform lastGrounded;
    public Transform LastGrounded { get { return lastGrounded; } set { lastGrounded = value;} }
    private OrderHandler playerHolding = null;
    public OrderHandler PlayerHolding { get {  return playerHolding; } }
    private OrderHandler playerDropped; // for cooldown with losing an order
    public OrderHandler PlayerDropped { get {  return playerDropped; } }

    [Tooltip("Arrow that points to the dropoff.")]
    [SerializeField] private GameObject arrow;

    [Tooltip("Time between a player dropping a package and being able to pick it back up again")]
    [SerializeField] private int pickupCooldown = 3;
    private bool canPickup = true; // if a player can pickup this order
    public bool CanPickup { get { return canPickup; } }
    [Tooltip("Default height of the order")]
    [SerializeField] private float height = 4.43f;

    [Tooltip("Reference to the beacon on this prefab")]
    [SerializeField] private OrderBeacon beacon;

    [Tooltip("Reference to the compass marker component on this object")]
    [SerializeField] CompassMarker compassMarker;
    [SerializeField] Sprite[] possiblePackageTypes;

    [Tooltip("The HDR color options for the different tiers of packages")]
    [SerializeField][ColorUsageAttribute(true, true)]public Color[] packageColors;

    [Tooltip("Different meshes of the package depending on the difficulty")]
    [SerializeField] private Mesh[] orderMesh;

    [Tooltip("Layermasks for respawn logic. Should be set to building phase checker, water (ignore raycast), and ground")]
    [SerializeField] LayerMask water,ground,buildingCheck;

    private IEnumerator pickupCooldownCoroutine; // IEnumerator reference for pickupCooldown coroutine

    private void Update()
    {
        meshRenderer.enabled = isActive;
        beacon.gameObject.SetActive(isActive);
        if (playerHolding != null)
        {
            this.gameObject.transform.forward = -playerHolding.transform.right;
        }

        Vector3 newDir = (arrow.transform.position - dropoff.position);
        newDir = new Vector3(newDir.x, 0, newDir.z);
        arrow.transform.rotation = Quaternion.LookRotation(newDir, Vector3.up) * Quaternion.Euler(0,1,0);
    }


    /// <summary>
    /// This method initializes the order with passed in values and sets its default location. It also initializes the beacon for this order.
    /// </summary>
    /// <param name="inPickup">Order's pickup point</param>
    /// <param name="inDropoff">Order's dropoff point</param>
    /// <param name="inValue">Value of the order</param>
    public void InitOrder()
    {
        OrderManager.Instance.AddOrder(this);
        isActive = true;
        arrow.SetActive(false);
        this.transform.position = pickup.position;
        
        // setting the color of the order
        //meshRenderer = GetComponent<MeshRenderer>();

        // Determines the pacakge type value based on the order type
        int packageType = 0;
        switch (value)
        {
            case Constants.OrderValue.Easy:
                packageType = 0;
                break;
            case Constants.OrderValue.Medium:
                packageType = 1;
                break;
            case Constants.OrderValue.Hard:
                packageType = 2;
                break;
            case Constants.OrderValue.Golden:
                packageType = 3;
                break;
        }

        meshRenderer.material.color = packageColors[packageType];
        meshFilter.mesh = orderMesh[packageType];

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
        this.gameObject.transform.rotation = Quaternion.identity;
        arrow.SetActive(true);
        playerHolding = player;
        if(value == Constants.OrderValue.Golden)
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
        arrow.SetActive(false);
        RaycastHit hit;
        bool foundSpot = false;
        transform.LookAt(Vector3.zero);

        if (value == Constants.OrderValue.Golden)
        {
            playerHolding.HasGoldenOrder = false;
        }
        else
        {
            // adjusts position if necessary
            for (int i = 0; i < 20; i++)
            {
                if (Physics.Raycast(newPosition, Vector3.down, out hit, Mathf.Infinity, ground) && Physics.Raycast(newPosition, Vector3.down, out hit, Mathf.Infinity, water)
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
        }
        transform.position = newPosition;
        this.transform.parent = OrderManager.Instance.transform;
        beacon.ResetPickup();
        
        playerDropped = playerHolding;
        playerHolding = null;
        StartPickupCooldownCoroutine();
    }
    /// <summary>
    /// This method performs the first half of the delivery, basically just hands the order to the customer.
    /// </summary>
    public void DeliverOrder()
    {
        arrow.SetActive(false);
        beacon.ThrowOrder(0.25f); // hard coded value for throwing the order to a customer
        OrderManager.Instance.IncrementCounters(value, -1);
        OrderManager.Instance.RemoveOrder(this);
    }

    /// <summary>
    /// This method fully erases the order so it's available in the pool.
    /// </summary>
    public void EraseOrder()
    {
        arrow.SetActive(false);
        
        // Removes the ui from all players
        compassMarker.RemoveCompassUIFromAllPlayers();
        
        OrderManager.Instance.IncrementCounters(value, -1);
        OrderManager.Instance.RemoveOrder(this);

        if (value == Constants.OrderValue.Golden)
        {
            if (playerHolding != null)
            {
                playerHolding.Score += OrderManager.Instance.FinalOrderValue - (int)Constants.OrderValue.Golden; // beacon code already adds base gold value
                playerHolding.HasGoldenOrder = false;
            }
            OrderManager.Instance.GoldOrderDelivered(); // lets the OM know the golden order has been delivered
        }
        
        if (playerHolding != null)
        {
            playerHolding.LoseOrder(this);
        }
        
        isActive = false;
        transform.position = pickup.position;
    }

    /// <summary>
    /// This method erases the golden order without it being "delivered". Used for hotkey functionality.
    /// </summary>
    public void EraseGoldWithoutDelivering()
    {
        beacon.EraseBeacon();
        OrderManager.Instance.FinalOrderValue = (int)Constants.OrderValue.Golden;
        if (value == Constants.OrderValue.Golden)
        {
            if (playerHolding != null)
            {
                playerHolding.HasGoldenOrder = false;
                playerHolding.LoseOrder(this);
            }
            compassMarker.RemoveCompassUIFromAllPlayers();
            OrderManager.Instance.IncrementCounters(value, -1);
            OrderManager.Instance.RemoveOrder(this);
            isActive = false;
        }
        transform.position = pickup.position;
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

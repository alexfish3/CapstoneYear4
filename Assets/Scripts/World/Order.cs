using DG.Tweening;
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
    [Header("Mesh Information")]
    [Tooltip("Actual mesh of the order for rotation and swapping models.")]
    [SerializeField] private GameObject orderMeshObject;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    [Tooltip("Scale of the mesh based on order difficulty.")]
    [SerializeField]private float[] meshScale = new float[4];

    [Header("Order Information")]
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
    [SerializeField] private float pickupCooldown = 3;
    private bool canPickup = true; // if a player can pickup this order
    public bool CanPickup { get { return canPickup; } }
    [Tooltip("Default height of the order")]
    [SerializeField] private float height = 4.43f;

    [Tooltip("Reference to the beacon on this prefab")]
    [SerializeField] private OrderBeacon beacon;

    [Tooltip("Reference to the compass marker component on this object")]
    public CompassMarker compassMarker;

    [Header("Order Movement When Holding")]
    [Tooltip("The amount of time it takes to rotate the order.")]
    [SerializeField] private float rotationDuration;
    [Tooltip("The amount the order will rotation per duration.")]
    [SerializeField] private Vector3 meshRotation;
    [Tooltip("How long it takes for the order to bob between positions.")]
    [SerializeField] private float bobbingDuration;
    [Tooltip("Bob offset of original order position.")]
    [SerializeField] private Vector3 bobPosition;

    // tweening
    private Tween floatyTween, bobbyTween, arrowTween;
    private Quaternion initMeshRotation;

    [Header("Order Type Information")]
    [SerializeField] Sprite[] possiblePackageTypes;

    [Tooltip("The HDR color options for the different tiers of packages")]
    [SerializeField][ColorUsageAttribute(true, true)]public Color[] packageColors;

    [Tooltip("Materials for different orders")]
    [SerializeField] private Material[] orderMaterials;

    [Tooltip("Different meshes of the package depending on the difficulty")]
    [SerializeField] private Mesh[] orderMesh;

    private IEnumerator pickupCooldownCoroutine; // IEnumerator reference for pickupCooldown coroutine

    private void Start()
    {
        meshRenderer = orderMeshObject.GetComponent<MeshRenderer>();
        meshFilter = orderMeshObject.GetComponent<MeshFilter>();
        initMeshRotation = orderMeshObject.transform.localRotation;
    }

    private void Update()
    {
        meshRenderer.enabled = isActive;
        beacon.gameObject.SetActive(isActive);
        if (playerHolding != null)
        {
            //this.gameObject.transform.forward = -playerHolding.transform.right;
        }

        Vector3 newDir = (arrow.transform.position - dropoff.position);
        newDir = new Vector3(newDir.x, 0, newDir.z);
        arrow.transform.rotation = Quaternion.LookRotation(newDir, Vector3.up) * Quaternion.Euler(0,1,0);
    }


    /// <summary>
    /// This method initializes this order to be ready for pickup. It also initializes the beacon for this order.
    /// </summary>
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

        meshRenderer.material = orderMaterials[packageType];
        meshFilter.mesh = orderMesh[packageType];
        orderMeshObject.transform.localScale = new Vector3(meshScale[packageType], meshScale[packageType], meshScale[packageType]);

        beacon.InitBeacon(this, packageType);
        compassMarker.icon = possiblePackageTypes[packageType];
        compassMarker.InitalizeCompassUIOnAllPlayers();
    }
    
    /// <summary>
    /// This method is called when the order is picked up by a player.
    /// </summary>
    public void Pickup(OrderHandler player)
    {
        //this.gameObject.transform.rotation = Quaternion.identity;
        arrow.SetActive(true);
        playerHolding = player;

        // Removes the ui from all players
        //compassMarker.RemoveCompassUIFromAllPlayers();

        if (value == Constants.OrderValue.Golden)
        {
            playerHolding.HasGoldenOrder = true;
        }
        playerDropped = null;
        beacon.SetDropoff(dropoff);
        
        compassMarker.SwitchCompassUIForPlayers(true);

        // start tweening
        floatyTween = orderMeshObject.transform.DORotate(meshRotation, rotationDuration, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetRelative()
            .SetEase(Ease.Linear);
        bobbyTween = orderMeshObject.transform.DOLocalMove(bobPosition, bobbingDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRelative()
            .SetEase(Ease.Linear);
        arrowTween = arrow.transform.DOLocalMove(bobPosition, bobbingDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRelative()
            .SetEase(Ease.Linear);
    }

    /// <summary>
    /// This method is "throws" the order in the air and then reinits it once the DOTween is complete. Meant for stealing.
    /// </summary>
    public void Drop(Vector3 newPosition)
    {
        floatyTween.Kill();
        bobbyTween.Kill();
        arrowTween.Kill();

        this.transform.parent = OrderManager.Instance.transform;
        orderMeshObject.transform.rotation = initMeshRotation;

        //// Removes the ui from all players
        //compassMarker.RemoveCompassUIFromAllPlayers();

        arrow.SetActive(false);
        transform.LookAt(Vector3.zero);
        
        beacon.ToggleBeaconMesh(false);

        float height = Random.Range(1f, 10f);
        transform.position = newPosition + height * transform.up;

        transform.DOMoveY(newPosition.y, pickupCooldown)
            .SetEase(Ease.OutBounce).OnComplete(() => ReInitOrder(newPosition));
        StartPickupCooldownCoroutine();

    }

    /// <summary>
    /// Resets the properties of the order so it can be picked up again.
    /// </summary>
    /// <param name="newPosition"></param>
    private void ReInitOrder(Vector3 newPosition)
    {
        if (value == Constants.OrderValue.Golden)
        {
            playerHolding.HasGoldenOrder = false;
        }

        playerDropped = playerHolding;
        RemovePlayerHolding();
    }
    /// <summary>
    /// This method performs the first half of the delivery, basically just hands the order to the customer.
    /// </summary>
    public void DeliverOrder()
    {
        RemovePlayerHolding();
        arrow.SetActive(false);
        if (value != Constants.OrderValue.Golden)
        {
            beacon.ThrowOrder(0.25f); // hard coded value for throwing the order to a customer
        }
        else
        {
            EraseOrder(); // temp fix for dotween handoff bug
        }
    }

    /// <summary>
    /// This method fully erases the order so it's available in the pool.
    /// </summary>
    public void EraseOrder()
    {
        floatyTween.Kill();
        bobbyTween.Kill();
        arrowTween.Kill();

        orderMeshObject.transform.rotation = initMeshRotation;

        Debug.Log("Erase Order");
        DOTween.Kill(transform);
        arrow.SetActive(false);
        
        // Removes the ui from all players
        compassMarker.RemoveCompassUIFromAllPlayers();

        beacon.CompassMarker.RemoveCompassUIFromAllPlayers();

        OrderManager.Instance.IncrementCounters(value, -1);
        OrderManager.Instance.RemoveOrder(this);

        if (value == Constants.OrderValue.Golden)
        {
            if (playerHolding != null)
            {
                playerHolding.Score += OrderManager.Instance.FinalOrderValue - (int)Constants.OrderValue.Golden; // beacon code already adds base gold value
                playerHolding.HasGoldenOrder = false;
            }
            if (GameManager.Instance.MainState == GameState.FinalPackage) // gold order was legit delivered
            {
                OrderManager.Instance.GoldOrderDelivered(); // lets the OM know the golden order has been delivered
            }
        }
        
        if (playerHolding != null)
        {
            playerHolding.LoseOrder(this);
        }
        
        isActive = false;
        transform.position = pickup.position;

        Debug.Log("Erase Order Finished");
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
                RemovePlayerHolding();
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
            // Removes the ui from all players
            //compassMarker.RemoveCompassUIFromAllPlayers();
            //playerHolding.GetComponent<Compass>().RemoveCompassMarker(beacon.CompassMarker, );
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
        beacon.ResetPickup();
        if (pickupCooldownCoroutine != null)
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

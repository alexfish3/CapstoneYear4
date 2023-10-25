/// <summary>This class manages the order spawning system in Dead on Arrival. 
/// It will spawn easy, medium, and hard orders with a cooldown, and retains a reference to each order through a list.
/// It is a Singleton and extends the SingletonMonobehaviour class
/// </summary>

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class OrderManager : SingletonMonobehaviour<OrderManager>
{
    private bool canSpawnOrders = true; // whether or not the manager can spawn orders
    public bool CanSpawnOrders { get { return canSpawnOrders; } set { canSpawnOrders = value; } }

    [Header("Game Information")]
    [Tooltip("Reference to the Order GameObject prefab")]
    [SerializeField] private GameObject orderPrefab;

    [Tooltip("Maxium number of orders present in the scene at a time")]
    [SerializeField]
    private int maxEasy, maxMedium, maxHard; // didn't think this variable name through

    private int maxOrders; // total number of orders possible in a game
    private float easyPercentage, mediumPercentage, hardPercentage; // percentage of easy/medium/hard orders that should be in a game
    private int currEasy, currMedium, currHard; // the current number of easy/medium/hard orders in the game

    private bool cooledDown = true;

    [Tooltip("Cooldown between order spawns")]
    [SerializeField] private float cooldownTime = 5f;

    [Header("World Information")]
    [Tooltip("List of the pickup points for deliveries")]
    [SerializeField] private List<Transform> pickupWaypoints;

    [Tooltip("List of the dropoff points for deliveries")]
    [SerializeField] private List<Transform> dropoffWaypoints;

    private List<Order> orders = new List<Order>(); // list of all the orders in the game at any time

    [Tooltip("Minimum distances for respective difficulty")]
    [SerializeField]
    private int easyDistance, mediumDistance, hardDistance;

    private IEnumerator easySpawnCoroutine, mediumSpawnCoroutine, hardSpawnCoroutine; // coroutines for managing cooldowns of the order spawns

    private void Start()
    {
        // calculate the percentages and initialize the counters for each type of order
        maxOrders = maxEasy + maxMedium + maxHard;
        easyPercentage = (float)maxEasy / maxOrders;
        mediumPercentage = (float)maxMedium / maxOrders;
        hardPercentage = (float)maxHard / maxOrders;
        currEasy = 0;
        currMedium = 0;
        currHard = 0;
    }

    private void Update()
    {
        if (canSpawnOrders)
        {
            if (orders.Count > 0)
            {
                if (cooledDown)
                {
                    StopEasySpawn();
                    StopMediumSpawn();
                    StopHardSpawn();
                    if ((float)currEasy / orders.Count() < easyPercentage || currEasy == 0) // if the number of easy orders is below the quota
                    {
                        StartEasySpawn();
                    }
                    else if ((float)currMedium / orders.Count() < mediumPercentage || currMedium == 0) // same for the medium orders
                    {
                        StartMediumSpawn();
                    }
                    else if ((float)currHard / orders.Count() < hardPercentage || currHard == 0) // and the hard orders
                    {
                        StartHardSpawn();
                    }
                }
            }
            else // to be executed for the first order
            {
                StartEasySpawn();
            }
        }
        else
        {
            StopEasySpawn();
            StopMediumSpawn();
            StopHardSpawn();
        }
        canSpawnOrders = orders.Count() >= maxOrders ? false : true;
    }

    /// <summary>
    /// This method adds and order to the list of existing orders if that order isn't already on the list.
    /// </summary>
    /// <param name="order">Order to be added to the list</param>
    public void AddOrder(Order order)
    {
        if(!orders.Contains(order))
        {
            orders.Add(order);
        }
    }

    /// <summary>
    /// This method removes an order from the order list if it's in the list.
    /// </summary>
    /// <param name="order">Order to be removed</param>
    public void RemoveOrder(Order order)
    {
        if(orders.Contains(order))
        {
            orders.Remove(order);
        }
    }

    /// <summary>
    /// This method adds a pickup and dropoff waypoint to their respective lists if they aren't already in the lists. Meant to be used when destroying an order so another could use those waypoints.
    /// </summary>
    /// <param name="pickup">Pickup waypoint to be added to pickup list</param>
    /// <param name="dropoff">Dropoff waypoint to be added to dropoff list</param>
    public void AddPickupDropoff(Transform pickup, Transform dropoff)
    {
        if(!pickupWaypoints.Contains(pickup))
        {
            pickupWaypoints.Add(pickup);
        }
        if(!dropoffWaypoints.Contains(dropoff))
        {
            dropoffWaypoints.Add(dropoff);
        }
    }

    /// <summary>
    /// This method attempts to spawn an order based on a random pickup waypoint and its proximity to other dropoff waypoints. If it fails it will return without doing anything.
    /// </summary>
    /// <param name="value">Value of the package to be spawned (Easy, Medium, or Hard)</param>
    private void SpawnOrder(Order.Order_Value value)
    {
        Debug.Log(value + " order being spawned"); // FOR TESTING
        float minDist, maxDist; // maximum and minimum distances to the dropoff waypoints
        GameObject newOrderGO = null; // the GameObject we're creating

        while (newOrderGO == null)
        {
            switch (value) // determines max and min distances based on passed in value
            {
                case Order.Order_Value.Easy:
                    maxDist = mediumDistance;
                    minDist = easyDistance;
                    break;
                case Order.Order_Value.Medium:
                    maxDist = hardDistance;
                    minDist = mediumDistance;
                    break;
                default:
                    maxDist = Mathf.Infinity;
                    minDist = hardDistance;
                    break;
            }

            // checks for a possible spawn and dropoff location
            foreach(Transform pickup in pickupWaypoints)
            {
                foreach (Transform dropoff in dropoffWaypoints)
                {
                    float distance = Vector3.Distance(pickup.position, dropoff.position);
                    if (distance >= minDist && distance <= maxDist)
                    {
                        newOrderGO = Instantiate(orderPrefab, this.transform); // creates a new order, sets this as its parent
                        Order newOrder = newOrderGO.GetComponent<Order>(); // orderPrefab should have an Order script on it
                        newOrder.InitOrder(pickup, dropoff, value); // next couple of lines are just initialization of the new order
                        AddOrder(newOrder);
                        dropoffWaypoints.Remove(dropoff);
                        pickupWaypoints.Remove(pickup);
                        IncrementCounters(value, 1);
                        Debug.Log(value + " order successfully spawned"); // FOR TESTING
                        return; // since we've spawned and initialized the order, we can return
                    }
                }
            }
            Debug.Log(value + " order failed to spawn"); // FOR TESTING
            return; // since we've ran through every possible spawn, return without spawning anything
        }
    }

    /// <summary>
    /// This method increments one of three counters depending on the value passed in, and based on the amount given.
    /// </summary>
    /// <param name="value">Value of the counter you want to increment (Easy, Medium, Hard)</param>
    /// <param name="amount">Amount you want to increment by (typically +1 or -1)</param>
    public void IncrementCounters(Order.Order_Value value, int amount)
    {
        switch(value)
        {
            case Order.Order_Value.Easy:
                currEasy+=amount;
                break;
            case Order.Order_Value.Medium:
                currMedium+=amount;
                break;
            case Order.Order_Value.Hard:
                currHard+=amount;
                break;
            default:
                break;
        }
    }

    // methods to start/stop coroutines
    private void StartEasySpawn()
    {
        if(easySpawnCoroutine == null)
        {
            easySpawnCoroutine = EasySpawn();
            StartCoroutine(easySpawnCoroutine);
        }
    }

    private void StopEasySpawn()
    {
        if(easySpawnCoroutine != null)
        {
            StopCoroutine(easySpawnCoroutine);
            easySpawnCoroutine = null;
        }
    }

    private void StartMediumSpawn()
    {
        if (mediumSpawnCoroutine == null)
        {
            mediumSpawnCoroutine = MediumSpawn();
            StartCoroutine(mediumSpawnCoroutine);
        }
    }

    private void StopMediumSpawn()
    {
        if (mediumSpawnCoroutine != null)
        {
            StopCoroutine(mediumSpawnCoroutine);
            mediumSpawnCoroutine = null;
        }
    }

    private void StartHardSpawn()
    {
        if(hardSpawnCoroutine == null)
        {
            hardSpawnCoroutine = HardSpawn();
            StartCoroutine(hardSpawnCoroutine);
        }
    }

    private void StopHardSpawn()
    {
        if(hardSpawnCoroutine != null)
        {
            StopCoroutine(hardSpawnCoroutine);
            hardSpawnCoroutine = null;
        }
    }

    /// <summary>
    /// This coroutine attempts to spawn an easy order after the cooldown period has elapsed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator EasySpawn()
    {
        cooledDown = false;
        yield return new WaitForSeconds(cooldownTime);
        SpawnOrder(Order.Order_Value.Easy);
        cooledDown = true;
    }

    /// <summary>
    /// This coroutine attempts to spawn a medium order after the cooldown period has elapsed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator MediumSpawn()
    {
        cooledDown = false;
        yield return new WaitForSeconds(cooldownTime);
        SpawnOrder(Order.Order_Value.Medium);
        cooledDown = true;
    }

    /// <summary>
    /// This coroutine attempts to spawn a hard order after the cooldown period has elapsed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator HardSpawn()
    {
        cooledDown = false;
        yield return new WaitForSeconds(cooldownTime);
        SpawnOrder(Order.Order_Value.Hard);
        cooledDown = true;
    }
}

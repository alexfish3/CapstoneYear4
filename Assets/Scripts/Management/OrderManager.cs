/// <summary>This class manages the order spawning system in Dead on Arrival. 
/// It will spawn easy, medium, and hard orders with a cooldown, and retains a reference to each order through a list.
/// It is a Singleton and extends the SingletonMonobehaviour class
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManager : SingletonMonobehaviour<OrderManager>
{
    private bool canSpawnOrders = true; // whether or not the manager can spawn orders
    public bool CanSpawnOrders { get { return canSpawnOrders; } set { canSpawnOrders = value; } }

    [Header("Game Information")]
    [SerializeField] private Order finalOrder;
    [Tooltip("Maxium number of orders present in the scene at a time. Array represents different waves")]
    [SerializeField]
    private int[] maxEasy, maxMedium, maxHard; // didn't think this variable name through

    [Tooltip("Time it takes in seconds for a wave to be completed")]
    [SerializeField] private float waveLengthInSeconds = 20f;
    private int wave = 0;
    public int Wave { get { return wave; } }
    private int maxWave = 0;
    public int MaxWave { get { return maxWave; } }
    private float waveTimer = 0f;

    public float WaveTimer { get { return waveTimer; } }
    private bool spawnNormalPackages = false;

    private int maxOrders; // total number of orders possible in a game
    private float easyPercentage, mediumPercentage, hardPercentage; // percentage of easy/medium/hard orders that should be in a game
    private int currEasy, currMedium, currHard; // the current number of easy/medium/hard orders in the game

    private bool cooledDown = true;

    [Tooltip("Cooldown between order spawns")]
    [SerializeField] private float cooldownTime = 5f;

    [Tooltip("Multiplyer for final order increment. 1 will have it add a dollar to the value every second.")]
    [SerializeField] private float goldIncrementMultiplyer = 1f;

    [Tooltip("The master list of all orders in the game.")]
    [SerializeField] private List<Order> totalOrders;
    [SerializeField] private List<Order> easy;
    [SerializeField] private List<Order> medium;
    [SerializeField] private List<Order> hard;

    [Header("World Information")]
    [Tooltip("Waypoints for golden order")]
    [SerializeField] private Transform goldenPickup, goldenDropoff;
    private bool finalOrderActive = false;
    public bool FinalOrderActive { get { return finalOrderActive; } }
    private float finalOrderValue = (float)Constants.OrderValue.Golden;
    public int FinalOrderValue { get { return (int)finalOrderValue; } set { finalOrderValue = (float)value; } }

    private List<Order> activeOrders = new List<Order>(); // list of all the orders in the game at any time

/*    [Tooltip("Minimum distances for respective difficulty")]
    [SerializeField]
    private int easyDistance, mediumDistance, hardDistance;*/

    private IEnumerator easySpawnCoroutine, mediumSpawnCoroutine, hardSpawnCoroutine; // coroutines for managing cooldowns of the order spawns

    // wave event stuff
    private event Action OnDeleteActiveOrders;

    [Header("Debug")]
    [Tooltip("When checked will loop through waves so you can play forever")]
    [SerializeField] private bool waveResets;
    [Tooltip("On means the max orders are the only orders spawned in the wave, off means new orders will spawn as they are delivered")]
    [SerializeField] private bool scarcityMode;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapBegin += InitGame;
        GameManager.Instance.OnSwapFinalPackage += DisableSpawning;
        GameManager.Instance.OnSwapFinalPackage += SpawnFinalOrder;
        HotKeys.Instance.onIncrementWave += IncrementWave;
        HotKeys.Instance.onDecrementWave += DecrementWave;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapBegin -= InitGame;
        GameManager.Instance.OnSwapFinalPackage -= DisableSpawning;
        GameManager.Instance.OnSwapFinalPackage -= SpawnFinalOrder;
        HotKeys.Instance.onIncrementWave -= IncrementWave;
        HotKeys.Instance.onDecrementWave -= DecrementWave;
    }

    private void Start()
    {
        int[] waves = { maxEasy.Length, maxMedium.Length, maxHard.Length };
        maxWave = Mathf.Max(waves);

        for(int i=0;i<totalOrders.Count();i++)
        {
            switch (totalOrders[i].Value)
            {
                case Constants.OrderValue.Easy:
                    easy.Add(totalOrders[i]);
                    break;
                case Constants.OrderValue.Medium:
                    medium.Add(totalOrders[i]); 
                    break;
                case Constants.OrderValue.Hard:
                    hard.Add(totalOrders[i]);
                    break;
                default:
                    break;
            }
        }
    }

    private void Update()
    {
        if (finalOrder != null)
        {
            if (finalOrderActive && finalOrder.PlayerHolding != null)
            {
                finalOrderValue += Time.deltaTime;
            }
        }
        if (!spawnNormalPackages) { return; }

        if (waveTimer > 0 && !finalOrderActive)
        {
            if (canSpawnOrders)
            {
                if (activeOrders.Count >= 0)
                {
                    if (cooledDown)
                    {
                        StopEasySpawn();
                        StopMediumSpawn();
                        StopHardSpawn();
                        if (easy.Count > 0 && ((float)currEasy / activeOrders.Count() < easyPercentage || currEasy == 0)) // if the number of easy orders is below the quota
                        {
                            StartEasySpawn();
                        }
                        else if (medium.Count > 0 && ((float)currMedium / activeOrders.Count() < mediumPercentage || currMedium == 0)) // same for the medium orders
                        {
                            StartMediumSpawn();
                        }
                        else if (hard.Count > 0 && ((float)currHard / activeOrders.Count() < hardPercentage || currHard == 0)) // and the hard orders
                        {
                            StartHardSpawn();
                        }
                    }
                }
            }
            else
            {
                StopEasySpawn();
                StopMediumSpawn();
                StopHardSpawn();
                cooledDown = true;
            }
            canSpawnOrders = activeOrders.Count() >= maxOrders ? false : true;
        }
        else if(!finalOrderActive && waveTimer <= 0)
        {
            ResetWave();
            InitWave();
        }
        waveTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Calls when the Begin/MainLoop sequence begins
    /// </summary>
    private void InitGame()
    {
        finalOrder.EraseGoldWithoutDelivering();
        finalOrderActive = false;
        spawnNormalPackages = true;
        waveTimer = waveLengthInSeconds;
        wave = 0;
        canSpawnOrders = true;
        EnableSpawning();
    }

    /// <summary>
    /// Initializes the wave.
    /// </summary>
    public void InitWave()
    {
        waveTimer = waveLengthInSeconds;
        try
        {
            // calculate the percentages and initialize the counters for each type of order
            maxOrders = maxEasy[wave] + maxMedium[wave] + maxHard[wave];
        }
        catch
        {
            GameManager.Instance.SetGameState(GameState.FinalPackage);
        }

        if (!finalOrderActive)
        {
            easyPercentage = (float)maxEasy[wave] / maxOrders;
            mediumPercentage = (float)maxMedium[wave] / maxOrders;
            hardPercentage = (float)maxHard[wave] / maxOrders;
        }

        currEasy = 0;
        currMedium = 0;
        currHard = 0;
    }

    /// <summary>
    /// Enables the spawning of pacakges
    /// </summary>
    private void EnableSpawning()
    {
        InitWave();
        spawnNormalPackages = true;
    }

    /// <summary>
    /// Disables the spawning of pacakges
    /// </summary>
    private void DisableSpawning()
    {
        ResetWave();
        spawnNormalPackages = false;
    }

    /// <summary>
    /// This method adds and order to the list of existing orders if that order isn't already on the list.
    /// </summary>
    /// <param name="order">Order to be added to the list</param>
    public void AddOrder(Order order)
    {
        if(!activeOrders.Contains(order))
        {
            activeOrders.Add(order);
            OnDeleteActiveOrders += order.EraseOrder;
        }
    }

    /// <summary>
    /// This method removes an order from the order list if it's in the list.
    /// </summary>
    /// <param name="order">Order to be removed</param>
    public void RemoveOrder(Order order)
    {
        if(activeOrders.Contains(order))
        {
            switch(order.Value)
            {
                case Constants.OrderValue.Easy:
                    easy.Add(order);
                    break;
                case Constants.OrderValue.Medium:
                    medium.Add(order); 
                    break;
                case Constants.OrderValue.Hard:
                    hard.Add(order);
                    break;
                default:
                    break;
            }
            order.transform.parent = this.transform;
            activeOrders.Remove(order);
            OnDeleteActiveOrders -= order.EraseOrder;
        }
    }

    /// <summary>
    /// This method attempts to spawn an order based on a random pickup waypoint and its proximity to other dropoff waypoints. If it fails it will return without doing anything.
    /// </summary>
    /// <param name="value">Value of the package to be spawned (Easy, Medium, or Hard)</param>
    private void SpawnOrder(Constants.OrderValue value)
    {
        Order nextOrder;
        switch (value)
        {
            case (Constants.OrderValue.Easy):
                if (easy.Count == 0)
                {
                    return;
                }
                nextOrder = easy[0];
                easy.Remove(nextOrder);
                break;
            case (Constants.OrderValue.Medium):
                if (medium.Count == 0)
                {
                    return;
                }
                nextOrder = medium[0];
                medium.Remove(nextOrder);
                break;
            case (Constants.OrderValue.Hard):
                if(hard.Count == 0)
                {
                    return;
                }
                nextOrder = hard[0];
                hard.Remove(nextOrder);
                break;
            default: // placeholder ?
                return;
        }
        nextOrder.InitOrder();
    }

    /// <summary>
    /// This method increments one of three counters depending on the value passed in, and based on the amount given.
    /// </summary>
    /// <param name="value">Value of the counter you want to increment (Easy, Medium, Hard)</param>
    /// <param name="amount">Amount you want to increment by (typically +1 or -1)</param>
    public void IncrementCounters(Constants.OrderValue value, int amount)
    {
        if(amount == -1 && scarcityMode) { return; } // won't let you count down orders on scarcity mode
        switch(value)
        {
            case Constants.OrderValue.Easy:
                currEasy+=amount;
                break;
            case Constants.OrderValue.Medium:
                currMedium+=amount;
                break;
            case Constants.OrderValue.Hard:
                currHard+=amount;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Removes all packages and starts the next wave.
    /// </summary>
    private void ResetWave()
    {
        StopEasySpawn();
        StopMediumSpawn();
        StopHardSpawn();
        cooledDown = true;
        wave++;
    }

    private void IncrementWave()
    {
        ResetWave();
        InitWave();
    }
    private void DecrementWave()
    {
        ResetWave();
        wave -= 2; // never expected to have to decrease waves
        InitWave();
    }

    /// <summary>
    /// For spawning the final order.
    /// </summary>
    private void SpawnFinalOrder()
    {
        OnDeleteActiveOrders?.Invoke();
        finalOrderActive = true;
        finalOrder.InitOrder();
    }

    /// <summary>
    /// Called from the golden order GO when the it's been delivered.
    /// </summary>
    public void GoldOrderDelivered()
    {
        finalOrderActive = false;
        if(waveResets)
        {
            wave = 0;
            GameManager.Instance.SetGameState(GameState.Begin);
        }
        else
        {
            GameManager.Instance.SetGameState(GameState.Results);
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
        SpawnOrder(Constants.OrderValue.Easy);
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
        SpawnOrder(Constants.OrderValue.Medium);
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
        SpawnOrder(Constants.OrderValue.Hard);
        cooledDown = true;
    }
}

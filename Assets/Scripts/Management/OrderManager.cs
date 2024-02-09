/// <summary>This class manages the order spawning system in Dead on Arrival. 
/// It will spawn easy, medium, and hard orders with a cooldown, and retains a reference to each order through a list.
/// It is a Singleton and extends the SingletonMonobehaviour class
/// </summary>

using JetBrains.Annotations;
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

    private int currEasy, currMedium, currHard; // the current number of easy/medium/hard orders in the game

    private bool cooledDown = true;

    [Tooltip("Cooldown between order spawns")]
    [SerializeField] private float cooldownTime = 5f;

    [Tooltip("Multiplyer for final order increment. 1 will have it add a dollar to the value every second.")]
    [SerializeField] private float goldIncrementMultiplyer = 1f;

    [Tooltip("The master list of all non-golden orders in the game.")]
    [SerializeField] private List<Order> normalOrders;
    [Tooltip("The final order in the game.")]
    [SerializeField] private Order finalOrder;

    [Tooltip("The audio source of the clocktower for the bell chimes on new wave.")]
    [SerializeField] private AudioSource clockSource;

    // lists for each of the types of orders in each game (minus golden ofc)
    private List<Order> easy = new List<Order>();
    private List<Order> medium = new List<Order>();
    private List<Order> hard = new List<Order>();

    private bool finalOrderActive = false;
    public bool FinalOrderActive { get { return finalOrderActive; } }
    private float finalOrderValue = (float)Constants.OrderValue.Golden;
    public int FinalOrderValue { get { return (int)finalOrderValue; } set { finalOrderValue = (float)value; } }

    private List<Order> activeOrders = new List<Order>(); // list of all the orders in the game at any time
    //private List<Order> ordersThisWave = new List<Order>();

    private IEnumerator easySpawnCoroutine, mediumSpawnCoroutine, hardSpawnCoroutine; // coroutines for managing cooldowns of the order spawns

    // wave event stuff
    private event Action OnDeleteActiveOrders;

    [Header("Debug")]
    [Tooltip("When checked will loop through waves so you can play forever")]
    [SerializeField] private bool waveResets;
    [Tooltip("When true will randomize initial spawn order of all orders.")]
    [SerializeField] private bool randomizeWaves;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapStartingCutscene += InitGame;
        GameManager.Instance.OnSwapGoldenCutscene += DisableSpawning;
        GameManager.Instance.OnSwapGoldenCutscene += SpawnFinalOrder;
        HotKeys.Instance.onIncrementWave += IncrementWave;
        HotKeys.Instance.onDecrementWave += DecrementWave;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapStartingCutscene -= InitGame;
        GameManager.Instance.OnSwapGoldenCutscene -= DisableSpawning;
        GameManager.Instance.OnSwapGoldenCutscene -= SpawnFinalOrder;
        HotKeys.Instance.onIncrementWave -= IncrementWave;
        HotKeys.Instance.onDecrementWave -= DecrementWave;
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
                        if (easy.Count > 0 && (currEasy < maxEasy[wave] || currEasy == 0)) // if the number of easy orders is below the quota
                        {
                            StartEasySpawn();
                        }
                        else if (medium.Count > 0 && (currMedium < maxMedium[wave] || currMedium == 0)) // same for the medium orders
                        {
                            StartMediumSpawn();
                        }
                        else if (hard.Count > 0 && (currHard < maxHard[wave] || currHard == 0)) // and the hard orders
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
            canSpawnOrders = currEasy < maxEasy[wave] || currMedium < maxMedium[wave] || currHard < maxHard[wave];
        }
        else if(!finalOrderActive && waveTimer <= 0)
        {
            ResetWave();
            InitWave();
        }
        waveTimer -= Time.deltaTime;
    }

    /// <summary>
    /// This method populates the order lists and inits any orders that should be active on runtime.
    /// </summary>
    private void GetOrders()
    {
        int[] waves = { maxEasy.Length, maxMedium.Length, maxHard.Length };
        maxWave = Mathf.Max(waves);

        for (int i = 0; i < normalOrders.Count(); i++)
        {
            if (normalOrders[i].IsActive)
            {
                IncrementCounters(normalOrders[i].Value, 1);
                normalOrders[i].InitOrder();
            }
            else
            {
                switch (normalOrders[i].Value)
                {
                    case Constants.OrderValue.Easy:
                        easy.Add(normalOrders[i]);
                        break;
                    case Constants.OrderValue.Medium:
                        medium.Add(normalOrders[i]);
                        break;
                    case Constants.OrderValue.Hard:
                        hard.Add(normalOrders[i]);
                        break;
                    default:
                        break;
                }
            }
        }
        if (randomizeWaves)
        {
            ShuffleList(easy);
            ShuffleList(medium);
            ShuffleList(hard);
        }
    }

    /// <summary>
    /// Calls when the Begin/MainLoop sequence begins
    /// </summary>
    private void InitGame()
    {
        finalOrder.EraseGoldWithoutDelivering();
        OnDeleteActiveOrders?.Invoke();
        GetOrders();
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
            // if this throws an error we know the final order should spawn
            int maxOrders = maxEasy[wave] + maxMedium[wave] + maxHard[wave];
        }
        catch
        {
            GameManager.Instance.SetGameState(GameState.GoldenCutscene);
            finalOrderActive = true;
        }

        if (!finalOrderActive)
        {
            if (wave > 0)
            {
                SoundManager.Instance.PlaySFX("bells", clockSource);
            }
        }
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
        Debug.Log("Disable Spawning Start");
        ResetWave();
        spawnNormalPackages = false;

        Debug.Log("Disable Spawning Was Successful");
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
            activeOrders.Remove(order);
        }
        if(order.Value == Constants.OrderValue.Golden)
        {
            finalOrderActive = false;
        }
        order.transform.parent = this.transform;
        OnDeleteActiveOrders -= order.EraseOrder;
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
                nextOrder = easy.ElementAt(0);
                easy.Remove(nextOrder);
                break;
            case (Constants.OrderValue.Medium):
                if (medium.Count == 0)
                {
                    return;
                }
                nextOrder = medium.ElementAt(0);
                medium.Remove(nextOrder);
                break;
            case (Constants.OrderValue.Hard):
                if(hard.Count == 0)
                {
                    return;
                }
                nextOrder = hard.ElementAt(0);
                hard.Remove(nextOrder);
                break;
            default: // placeholder ?
                return;
        }
        IncrementCounters(value, 1);
        nextOrder.InitOrder();
    }

    /// <summary>
    /// This method increments one of three counters depending on the value passed in, and based on the amount given.
    /// </summary>
    /// <param name="value">Value of the counter you want to increment (Easy, Medium, Hard)</param>
    /// <param name="amount">Amount you want to increment by (typically +1 or -1)</param>
    public void IncrementCounters(Constants.OrderValue value, int amount)
    {
        //if(amount == -1 && scarcityMode) { return; } // won't let you count down orders on scarcity mode
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
    /// Starts the next wave.
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
        Debug.Log("Spawn Final Order Start");

        OnDeleteActiveOrders?.Invoke();
        finalOrderActive = true;
        finalOrder.InitOrder();

        Debug.Log("Spawn Final Order Was Successful");
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

    /// <summary>
    /// Shuffles the elements in a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="inList"></param>
    private void ShuffleList<T>(List<T> inList)
    {
        for(int i=0;i<inList.Count; i++)
        {
            T holder = inList[i];
            int r = UnityEngine.Random.Range(i, inList.Count);
            inList[i] = inList[r];
            inList[r] = holder;
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

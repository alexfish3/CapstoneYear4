using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInstantiate : SingletonMonobehaviour<PlayerInstantiate>
{
    const int MAX_PLAYERS = 4;
    const int MAX_REQUIRED_READY_UP = 2;

    [Header("Player References")]
    [SerializeField] int playerCount = 0;
    [SerializeField] PlayerInput[] avaliblePlayerInputs = new PlayerInput[MAX_PLAYERS];

    [Header("Ready Up Information")]
    [SerializeField] bool[] playerReadyUp = new bool[MAX_PLAYERS];
    public static UnityEvent OnReadiedUp;
    int readyUpCounter = 0;

    ///<summary>
    /// Event to add the player references and rename player
    ///</summary>
    public void AddPlayerReference(PlayerInput playerInput)
    {
        playerCount++;

        playerInput.gameObject.name = "Player " + playerCount.ToString();

        Debug.Log("Spawned Player " + playerCount);
        AddToPlayerArray(playerInput);

        // Temp, remove once we enable input ready up
        PlayerReady(playerInput);
    }

    ///<summary>
    /// Event to remove the player from references
    ///</summary>
    public void RemovePlayerReference(PlayerInput playerInput)
    {
        playerCount--;
    }

    ///<summary>
    /// Adds the player input to the player array
    ///</summary>
    public void AddToPlayerArray(PlayerInput playerInput)
    {
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
            {
                avaliblePlayerInputs[i] = playerInput;
                return;
            }
        }
    }

    ///<summary>
    /// Removes the player input from the player array
    ///</summary>
    public void RemoveFromPlayerArray(PlayerInput playerInput)
    {
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == playerInput)
            {
                avaliblePlayerInputs[i] = null;
                return;
            }
        }
    }

    ///<summary>
    /// Allows Player To Ready
    ///</summary>
    public void PlayerReady(PlayerInput playerInput)
    {
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == playerInput)
            {
                playerReadyUp[i] = true;
                return;
            }
        }

        // After readying up player, check if you can start game
        CheckReadyUpCount();
    }

    ///<summary>
    /// Allows Player To Not Ready
    ///</summary>
    public void PlayerNotReady(PlayerInput playerInput)
    {
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == playerInput)
            {
                playerReadyUp[i] = false;
                return;
            }
        }
    }

    ///<summary>
    /// Checks for how many players are readied up
    ///</summary>
    public void CheckReadyUpCount()
    {
        // Resets and loops to count amount of ready ups
        readyUpCounter = 0;
        foreach (bool readyUpValue in playerReadyUp)
        {
            if (readyUpValue)
                readyUpCounter++;
        }

        // Checks if number of readied up is greater then or equal to required amount
        if(readyUpCounter >= MAX_REQUIRED_READY_UP)
        {
            OnReadiedUp?.Invoke();
        }
    }

}

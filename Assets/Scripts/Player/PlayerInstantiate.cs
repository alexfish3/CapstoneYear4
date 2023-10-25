using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInstantiate : SingletonMonobehaviour<PlayerInstantiate>
{
    [Header("Player References")]
    [SerializeField] GameObject PlayerHolder;
    [SerializeField] int playerCount = 0;
    public int PlayerCount { get { return playerCount; } }

    [SerializeField] PlayerInput[] avaliblePlayerInputs = new PlayerInput[Constants.MAX_PLAYERS];
    public PlayerInput[] PlayerInputs { get { return avaliblePlayerInputs; } }

    [Header("Ready Up Information")]
    [SerializeField] bool[] playerReadyUp = new bool[Constants.MAX_PLAYERS];
    public event Action OnReadiedUp;
    int readyUpCounter = 0;

    ///<summary>
    /// Event to add the player references and rename player
    ///</summary>
    public void AddPlayerReference(PlayerInput playerInput)
    {
        playerCount++;

        playerInput.gameObject.name = "Player " + playerCount.ToString();
        Debug.Log("Spawned Player " + playerCount);

        playerInput.gameObject.transform.parent = PlayerHolder.transform;

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
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
            {
                avaliblePlayerInputs[i] = playerInput;
                break;
            }
        }
    }

    ///<summary>
    /// Removes the player input from the player array
    ///</summary>
    public void RemoveFromPlayerArray(PlayerInput playerInput)
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == playerInput)
            {
                avaliblePlayerInputs[i] = null;
                break;
            }
        }
    }

    ///<summary>
    /// Allows Player To Ready
    ///</summary>
    public void PlayerReady(PlayerInput playerInput)
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == playerInput)
            {
                playerReadyUp[i] = true;
                break;
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
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
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
        if (readyUpCounter >= Constants.MAX_REQUIRED_READY_UP)
        {
            // Requires max players and does not have max
            if (Constants.REQUIRE_MAX_PLAYERS && readyUpCounter < Constants.MAX_PLAYERS)
            {
                Debug.Log("Not Enough Readied Up");
                return;
            }

            Debug.Log("Properly Readied Up");

            OnReadiedUp?.Invoke();
        }
        else
        {
            Debug.Log("Not Enough Readied Up");
        }
    }

}

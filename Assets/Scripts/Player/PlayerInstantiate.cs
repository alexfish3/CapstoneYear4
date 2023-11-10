using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInstantiate : SingletonMonobehaviour<PlayerInstantiate>
{
    [Header("Player Info")]
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject playerHolder;

    [Space(10)]
    [SerializeField] bool allowPlayerSpawn = true;
    [SerializeField] int playerCount = 0;
    public int PlayerCount { get { return playerCount; } }

    [SerializeField] PlayerInput[] avaliblePlayerInputs = new PlayerInput[Constants.MAX_PLAYERS];
    public PlayerInput[] PlayerInputs { get { return avaliblePlayerInputs; } }

    [Header("Ready Up Information")]
    [SerializeField] bool[] playerReadyUp = new bool[Constants.MAX_PLAYERS];
    public event Action OnReadiedUp;
    int readyUpCounter = 0;

    ///<summary>
    /// OnEnable, where i set event methods
    ///</summary>
    public void OnEnable()
    {
        gameManager.OnSwapPlayerSelect += EnablePlayerSpawn;
        gameManager.OnSwapBegin += DisablePlayerSpawn;
    }

    ///<summary>
    /// OnDisable, where i set event methods
    ///</summary>
    public void OnDisable()
    {
        gameManager.OnSwapPlayerSelect -= EnablePlayerSpawn;
        gameManager.OnSwapBegin -= DisablePlayerSpawn;
    }

    ///<summary>
    /// Event to add the player references and rename player
    ///</summary>
    public void AddPlayerReference(PlayerInput playerInput)
    {
        // If player spawn is disabled
        if(allowPlayerSpawn == false && Constants.SPAWN_MID_MATCH == false)
        {
            Debug.Log("Disable Spawning");
            Destroy(playerInput.gameObject);
            return;
        }

        // Up the player count
        playerCount++;

        GameObject ColliderObject = playerInput.gameObject.GetComponentInChildren<SphereCollider>().gameObject;
        BallDriving ballDriving = playerInput.gameObject.GetComponentInChildren<BallDriving>();
        Camera baseCam = playerInput.camera;

        // Update tag of player
        switch (playerCount)
        {
            case 1:
                ColliderObject.layer = 10; // Player 1;
                ballDriving.playerIndex = 1;
                baseCam.cullingMask |= (1<<10);
                break;
            case 2:
                ColliderObject.layer = 11; // Player 2;
                ballDriving.playerIndex = 2;
                baseCam.cullingMask |= (1 << 11);
                break;
            case 3:
                ColliderObject.layer = 12; // Player 3;
                ballDriving.playerIndex = 3;
                baseCam.cullingMask |= (1 << 12);
                break;
            case 4:
                ColliderObject.layer = 13; // Player 4;
                ballDriving.playerIndex = 4;
                baseCam.cullingMask |= (1 << 13);
                break;
        }

        // Update the naming scheme of the input reciever
        playerInput.gameObject.name = "Player " + playerCount.ToString();
        playerInput.gameObject.transform.parent = playerHolder.transform;

        AddToPlayerArray(playerInput);

        // Updates all the player's cameras due to this new player
        UpdatePlayerCameraRects(playerCount);

        // Temp, remove once we enable input ready up
        PlayerReady(playerInput);
    }

    ///<summary>
    /// Updates the camera rects on all players in scenes
    ///</summary>
    private void UpdatePlayerCameraRects(int playerCount)
    {
        Rect[] cameraRects = CalculateRects(playerCount);

        for(int i = 0; i < cameraRects.Length; i++)
        {
            PlayerInputs[i].camera.rect = cameraRects[i];
        }
    }

    ///<summary>
    /// Calculates the camera rects for when there are 1 - 4 players
    ///</summary>
    private Rect[] CalculateRects(int playerCount)
    {
        Rect[] viewportRects = new Rect[playerCount];
        
        // 1 Player
        if(playerCount == 1)
        {
            viewportRects[0] = new Rect(0, 0, 1, 1);
        }
        else if (playerCount == 2)
        {
            viewportRects[0] = new Rect(0.25f, 0.5f, 0.5f, 0.5f);
            viewportRects[1] = new Rect(0.25f, 0, 0.5f, 0.5f);
        }
        else if (playerCount == 3)
        {
            viewportRects[0] = new Rect(0, 0.5f, 0.5f, 0.5f);
            viewportRects[1] = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            viewportRects[2] = new Rect(0.25f, 0, 0.5f, 0.5f);
        }
        else if (playerCount == 4)
        {
            viewportRects[0] = new Rect(0, 0.5f, 0.5f, 0.5f);
            viewportRects[1] = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            viewportRects[2] = new Rect(0, 0, 0.5f, 0.5f);
            viewportRects[3] = new Rect(0.5f, 0, 0.5f, 0.5f);
        }

        return viewportRects;
    }

    ///<summary>
    /// Event to remove the player from references
    ///</summary>
    public void RemovePlayerReference(PlayerInput playerInput)
    {
        // If player spawn is disabled
        if (allowPlayerSpawn == false && Constants.SPAWN_MID_MATCH == false)
        {
            return;
        }

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

    ///<summary>
    /// Enables the ability to spawn players
    ///</summary>
    public void EnablePlayerSpawn() { allowPlayerSpawn = true;}

    ///<summary>
    /// Disables the ability to spawn players
    ///</summary>
    public void DisablePlayerSpawn() { allowPlayerSpawn = false; }

}

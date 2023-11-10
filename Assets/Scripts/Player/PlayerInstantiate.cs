using System;
using System.Collections;
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
    [SerializeField] bool isReadedUp = false;
    [SerializeField] float countdownTimer = 5f;
    Coroutine readyUpCountdown;
        

    ///<summary>
    /// OnEnable, where i set event methods
    ///</summary>
    public void OnEnable()
    {
        gameManager.OnSwapPlayerSelect += EnablePlayerSpawn;

        gameManager.OnSwapBegin += DisablePlayerSpawn;
        gameManager.OnSwapBegin += SwapPlayerControlSchemeToDrive;

        gameManager.OnSwapResults += DisableReadiedUp;
        gameManager.OnSwapResults += SwapPlayerControlSchemeToUI;
    }

    ///<summary>
    /// OnDisable, where i set event methods
    ///</summary>
    public void OnDisable()
    {
        gameManager.OnSwapPlayerSelect -= EnablePlayerSpawn;

        gameManager.OnSwapBegin -= DisablePlayerSpawn;
        gameManager.OnSwapBegin -= SwapPlayerControlSchemeToDrive;

        gameManager.OnSwapResults -= DisableReadiedUp;
        gameManager.OnSwapResults -= SwapPlayerControlSchemeToUI;
    }

    public void Update()
    {
        if(isReadedUp == true)
        {
            return;
        }

        CheckReadyUpCount();
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

        SwapPlayerControlSchemeToUI();
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

        // Checks if players are greater then 1 and all players are readied up
        if (readyUpCounter >= playerCount && playerCount >= 1)
        {
            if(readyUpCountdown == null)
                readyUpCountdown = StartCoroutine(ReadyUpCountdown());
        }
        else
        {
            return;
        }
    }

    private IEnumerator ReadyUpCountdown()
    {
        for(float i = countdownTimer; i > 0; i--)
        {
            Debug.Log(i);
            yield return new WaitForSeconds(countdownTimer / countdownTimer);
        }

        Debug.Log("Properly Readied Up");
        isReadedUp = true;
        OnReadiedUp?.Invoke();
        readyUpCountdown = null;
    }


    ///<summary>
    /// Enables the ability to spawn players
    ///</summary>
    public void EnablePlayerSpawn() { allowPlayerSpawn = true;}

    ///<summary>
    /// Disables the ability to spawn players
    ///</summary>
    public void DisablePlayerSpawn() { allowPlayerSpawn = false; }

    ///<summary>
    /// Swaps all player's control schemes to UI
    ///</summary>
    public void SwapPlayerControlSchemeToUI()
    {
        for (int i = 0; i < playerCount; i++)
        {
            avaliblePlayerInputs[i].actions.FindActionMap("UI").Enable();
            avaliblePlayerInputs[i].actions.FindActionMap("Player").Disable();
        }
    }

    ///<summary>
    /// Swaps all player's control schemes to "Driving" Player movement
    ///</summary>
    public void SwapPlayerControlSchemeToDrive()
    {
        for (int i = 0; i < playerCount; i++)
        {
            avaliblePlayerInputs[i].actions.FindActionMap("UI").Disable();
            avaliblePlayerInputs[i].actions.FindActionMap("Player").Enable();
        }
    }

    public void readyUp(int playerIndexToReadyUp) 
    { 
        playerReadyUp[playerIndexToReadyUp] = true;
        CheckReadyUpCount();
    }
    public void unreadyUp(int playerIndexToReadyUp) 
    { 
        playerReadyUp[playerIndexToReadyUp] = false; 
        if(readyUpCountdown != null)
        {
            StopCoroutine(readyUpCountdown);
            readyUpCountdown = null;
        }
    }

    ///<summary>
    /// Disables all player's bools of readied up
    ///</summary>
    public void DisableReadiedUp() 
    { 
        isReadedUp = false;
        
        for(int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            playerReadyUp[i] = false;
        }

    }

}

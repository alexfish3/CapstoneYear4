using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerInstantiate : SingletonMonobehaviour<PlayerInstantiate>
{
    [SerializeField]
    Rect[] cameraRects;

    [Header("Player Info")]
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject playerHolder;

    [Space(10)]
    [Tooltip("Enables or disables the ability for players to spawn into the lobby")]
    [SerializeField] bool allowPlayerSpawn = true;

    [Tooltip("This value is our current player count, ie the # of players in match")]
    [SerializeField] int playerCount = 0;
    public int PlayerCount { get { return playerCount; } }

    [Tooltip("the list of avalible player input class objects")]
    [SerializeField] PlayerInput[] avaliblePlayerInputs = new PlayerInput[Constants.MAX_PLAYERS];
    public PlayerInput[] PlayerInputs { get { return avaliblePlayerInputs; } }
    [Tooltip("The indexed array of player spawn positions")]
    [SerializeField] GameObject[] menuSpawnPositions = new GameObject[Constants.MAX_PLAYERS];

    [Tooltip("The indexed array of player render texutres")]
    [SerializeField] RenderTexture[] playerRenderTextures = new RenderTexture[Constants.MAX_PLAYERS];

    [Header("Ready Up Information")]
    [Tooltip("The indexed array tracking players' ready up status")]
    [SerializeField] bool[] playerReadyUp = new bool[Constants.MAX_PLAYERS];
    public event Action OnReadiedUp;
    int readyUpCounter = 0;
    [Tooltip("Set to true when all players are readied up")]
    [SerializeField] bool isAllReadedUp = false;
    [Tooltip("The time it takes to start the match")]
    [SerializeField] float countdownTimer = 5f;
    Coroutine readyUpCountdown;

    CutsceneManager cutsceneManager;
       
    ///<summary>
    /// OnEnable, where i set event methods
    ///</summary>
    public void OnEnable()
    {
        gameManager.OnSwapPlayerSelect += EnablePlayerSpawn;
        gameManager.OnSwapPlayerSelect += SwapForCharacterSelect;

        gameManager.OnSwapStartingCutscene += DisablePlayerSpawn;
        gameManager.OnSwapStartingCutscene += PlayerUpdateDrivingIndicators;

        gameManager.OnSwapBegin += SwapForDriving;

        gameManager.OnSwapStartingCutscene += SwapForCutscene;
        gameManager.OnSwapGoldenCutscene += SwapForCutscene;
        gameManager.OnSwapGoldenCutscene += PlayerUpdateDrivingIndicators;

        gameManager.OnSwapFinalPackage += SwapForDriving;

        gameManager.OnSwapResults += DisableReadiedUp;
        gameManager.OnSwapResults += ResetPlayerCanvas;
        gameManager.OnSwapResults += SwapForResults;

        gameManager.OnSwapMenu += SwapForMainMenu;

    }

    ///<summary>
    /// OnDisable, where i set event methods
    ///</summary>
    public void OnDisable()
    {
        gameManager.OnSwapPlayerSelect -= EnablePlayerSpawn;
        gameManager.OnSwapPlayerSelect -= SwapForCharacterSelect;

        gameManager.OnSwapStartingCutscene -= DisablePlayerSpawn;
        gameManager.OnSwapStartingCutscene -= PlayerUpdateDrivingIndicators;

        gameManager.OnSwapBegin -= SwapForDriving;

        gameManager.OnSwapStartingCutscene -= SwapForCutscene;
        gameManager.OnSwapGoldenCutscene -= SwapForCutscene;
        gameManager.OnSwapGoldenCutscene -= PlayerUpdateDrivingIndicators;

        gameManager.OnSwapFinalPackage -= SwapForDriving;

        gameManager.OnSwapResults -= DisableReadiedUp;
        gameManager.OnSwapResults -= ResetPlayerCanvas;
        gameManager.OnSwapResults -= SwapForResults;

        gameManager.OnSwapMenu -= SwapForMainMenu;
    }

    public void Update()
    {

    }

    ///<summary>
    /// Event to add the player references and rename player
    ///</summary>
    public void AddPlayerReference(PlayerInput playerInput)
    {
        // If player spawn is disabled
        if(allowPlayerSpawn == false && playerCount >= 1)
        {
            Debug.Log("Disable Spawning");
            Destroy(playerInput.gameObject);
            return;
        }

        Debug.Log("Added Player");

        if (playerCount <= 0)
        {
            Debug.Log("Spawn Host Player");
            playerInput.gameObject.GetComponent<PlayerUIHandler>().menuInteractions.hostPlayer = true;
            playerInput.gameObject.GetComponent<PlayerUIHandler>().menuInteractions.SwapMenuType(MenuType.MainMenu);

            MainMenu.Instance.Player1ControllerConnected(playerInput);
        }
        else
        {
            Debug.Log("Spawn Sub Player");
            playerInput.gameObject.GetComponent<PlayerUIHandler>().menuInteractions.SwapToPlayerSelect();
        }

        // Up the player count
        playerCount++;

        GameObject ColliderObject = playerInput.gameObject.GetComponentInChildren<SphereCollider>().gameObject;
        BallDriving ballDriving = playerInput.gameObject.GetComponentInChildren<BallDriving>();
        Camera baseCam = playerInput.camera;
        PlayerCameraResizer playerCameraResizer = playerInput.gameObject.GetComponentInChildren<PlayerCameraResizer>();

        int nextFillSlot = 0;

        for(int i = 0; i < avaliblePlayerInputs.Length; i++)
        {
            if (avaliblePlayerInputs[i] == null)
            {
                nextFillSlot = i + 1;
                break;
            }
        }

        // Update tag of player
        switch (nextFillSlot)
        {
            case 1:
                ColliderObject.layer = 10; // Player 1;
                ballDriving.playerIndex = 1;
                baseCam.cullingMask |= (1 << 10);
                playerCameraResizer.PlayerRenderCamera.targetTexture = playerRenderTextures[0];
                break;
            case 2:
                ColliderObject.layer = 11; // Player 2;
                ballDriving.playerIndex = 2;
                baseCam.cullingMask |= (1 << 11);
                playerCameraResizer.PlayerRenderCamera.targetTexture = playerRenderTextures[1];
                break;
            case 3:
                ColliderObject.layer = 12; // Player 3;
                ballDriving.playerIndex = 3;
                baseCam.cullingMask |= (1 << 12);
                playerCameraResizer.PlayerRenderCamera.targetTexture = playerRenderTextures[2];
                break;
            case 4:
                ColliderObject.layer = 13; // Player 4;
                ballDriving.playerIndex = 4;
                baseCam.cullingMask |= (1 << 13);
                playerCameraResizer.PlayerRenderCamera.targetTexture = playerRenderTextures[3];
                break;
        }

        // Updates the main virtual camera based on the player number
        playerCameraResizer.UpdateMainVirtualCameras(nextFillSlot);

        // Updates the icon virtual cameras based on the player number
        playerCameraResizer.UpdateIconVirtualCameras(nextFillSlot);

        // Relocates the customization menu based on player number
        playerCameraResizer.RelocateCustomizationMenu(nextFillSlot);

        // Update the naming scheme of the input reciever
        playerInput.gameObject.name = "Player " + nextFillSlot.ToString();
        playerInput.gameObject.transform.parent = playerHolder.transform;

        AddToPlayerArray(playerInput);

        // Updates all the player's cameras due to this new player
        UpdatePlayerCameraRects();

        // Swaps the player's control scheme to UI
        SwapPlayerControlSchemeToUI();

        // Sets the player's spawn point above ground on map
        SetAllPlayerSpawn();

        // Unreadys up the player joining, which stops the countdown if it is currently counting
        UnreadyUp(nextFillSlot - 1);

        // Disables text for fillslot text based on player's fill slot
        PlayerSelectCanvas.Instance.TogglePressButtonTexts(nextFillSlot - 1, false);
    }

    ///<summary>
    /// Spawns each player at a set position
    ///</summary>
    public void SetAllPlayerSpawn()
    {
        Debug.Log("Spawn Players at right positions");

        // Loops for all spawned players
        for (int i = 0; i < avaliblePlayerInputs.Length; i++)
        {
            if (avaliblePlayerInputs[i] == null)
                continue;

            // Resets the velocity of the players
            avaliblePlayerInputs[i].GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;

            // reset position and rotation of ball and controller
            avaliblePlayerInputs[i].GetComponentInChildren<Rigidbody>().transform.position = menuSpawnPositions[i].transform.position;
            avaliblePlayerInputs[i].GetComponentInChildren<Rigidbody>().transform.rotation = menuSpawnPositions[i].transform.rotation;

            avaliblePlayerInputs[i].GetComponentInChildren<BallDriving>().transform.position = menuSpawnPositions[i].transform.position;
            avaliblePlayerInputs[i].GetComponentInChildren<BallDriving>().transform.rotation = menuSpawnPositions[i].transform.rotation;
        }
    }

    ///<summary>
    /// Updates the camera rects on all players in scenes
    ///</summary>
    private void UpdatePlayerCameraRects()
    {
        cameraRects = CalculateRects();

        if (cameraRects.Length <= 0)
            return;

        int cameraRectCounter = 0;

        for (int i = 0; i < avaliblePlayerInputs.Length; i++)
        {
            if (avaliblePlayerInputs[i] != null)
            {
                if (cameraRectCounter < cameraRects.Length)
                {
                    Debug.Log("Resize Camera");
                    Rect temp = cameraRects[cameraRectCounter];
                    avaliblePlayerInputs[i].camera.rect = temp;
                    cameraRectCounter++;
                }
                else
                {
                    // Handle the case where there are more non-null player inputs than camera rects
                    Debug.LogWarning("Not enough camera rects for all players.");
                    break;
                }
            }
        }
    }

    ///<summary>
    /// Calculates the camera rects for when there are 1 - 4 players
    ///</summary>
    private Rect[] CalculateRects()
    {
        Debug.LogWarning("Resizing UI, player count is now " + playerCount);

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
    public void RemovePlayerRef(PlayerInput playerInput)
    {
        Debug.Log("Remove Player");

        // If player spawn is disabled
        if (allowPlayerSpawn == false && Constants.SPAWN_MID_MATCH == false)
        {
            return;
        }

        int position = RemoveFromPlayerArray(playerInput);

        //Enabled text for fillslot text based on player's removed position
        PlayerSelectCanvas.Instance.TogglePressButtonTexts(position, true);

        ScoreManager.Instance.UpdateOrderHandlers(avaliblePlayerInputs);
        QAManager.Instance.UpdateQAHandlers(avaliblePlayerInputs);
        
        Destroy(playerInput.gameObject);

        UpdatePlayerCameraRects();
    }

    public void SubtractPlayerCount()
    {
        playerCount--;
    }

    ///<summary>
    /// Destroys all connected players
    ///</summary>
    public void ClearPlayerArray()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] != null)
            {
                Destroy(avaliblePlayerInputs[i].gameObject);
                avaliblePlayerInputs[i] = null;
                playerCount--;
            }
        }
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
    /// Removes the player input from the player array, returns position where it was removed
    ///</summary>
    public int RemoveFromPlayerArray(PlayerInput playerInput)
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == playerInput)
            {
                avaliblePlayerInputs[i] = null;
                return i;
            }
        }
        return 0;
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

    ///<summary>
    /// The countdown ready-up coroutine that begins the game
    ///</summary>
    private IEnumerator ReadyUpCountdown()
    {
        for(float i = countdownTimer; i >= 0; i--)
        {
            PlayerSelectCanvas.Instance.UpdateCountdown(i + 1);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Properly Readied Up");
        PlayerSelectCanvas.Instance.UpdateCountdown(0);
        isAllReadedUp = true;
        OnReadiedUp?.Invoke();
        readyUpCountdown = null;
    }

    /// <summary>
    /// Swaps the control scheme for all players to main menu
    /// </summary>
    private void SwapForMainMenu()
    {
        Debug.Log("Swap for Main Menu");

        SwapPlayerControlSchemeToUI();

        SwapMenuTypeForAllPlayers(MenuType.MainMenu);
    }

    /// <summary>
    /// Swaps the control scheme for all players to cutscenes
    /// </summary>
    private void SwapForCutscene()
    {
        Debug.Log("Swap for Cutscene");

        SwapPlayerControlSchemeToUI();

        SwapMenuTypeForAllPlayers(MenuType.Cutscene);
    }

    /// <summary>
    /// Swaps the control scheme for all players to character select
    /// </summary>
    private void SwapForCharacterSelect()
    {
        Debug.Log("Swap for Charater Select");

        SwapPlayerControlSchemeToUI();

        SwapMenuTypeForAllPlayers(MenuType.PlayerSelect);
    }

    /// <summary>
    /// Swaps the control scheme for all players to results
    /// </summary>
    private void SwapForResults()
    {
        Debug.Log("Swap for Results");

        SwapPlayerControlSchemeToUI();

        SwapMenuTypeForAllPlayers(MenuType.ResultsMenu);
    }

    /// <summary>
    /// Swaps the control scheme for all players to driving
    /// </summary>
    private void SwapForDriving()
    {
        StartCoroutine(PlayerMoverCountdown());
    }

    private IEnumerator PlayerMoverCountdown()
    {
        if (cutsceneManager == null)
            cutsceneManager = CutsceneManager.Instance;

        Debug.Log("Swap for Driving");

        cutsceneManager.BeginCountdownAnimation();

        yield return new WaitForSeconds(3f);
        Debug.Log("Waited Three seconds");

        SwapPlayerControlSchemeToDrive();
        SwapMenuTypeForAllPlayers(MenuType.PauseMenu);
    }

    /// <summary>
    /// Swaps the menu type for all players
    /// </summary>
    private void SwapMenuTypeForAllPlayers(MenuType menuType)
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
                continue;

            avaliblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().menuInteractions.SwapMenuType(menuType);

            // If swapping to pause menu, reparent menu ui to game-camera
            if (menuType == MenuType.PauseMenu)
            {
                avaliblePlayerInputs[i].gameObject.GetComponent<PlayerCameraResizer>().ReparentMenuCameraStack(true);
            }
            // If swapping to other menu, reparent menu ui to player-camera on main menu
            else if (menuType == MenuType.MainMenu)
            {
                avaliblePlayerInputs[i].gameObject.GetComponent<PlayerCameraResizer>().ReparentMenuCameraStack(false);
            }
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

    ///<summary>
    /// Swaps all player's control schemes to UI
    ///</summary>
    public void SwapPlayerControlSchemeToUI()
    {
        Debug.Log("<color=green>Swapping To UI Controls</color>");

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
                continue;

            avaliblePlayerInputs[i].gameObject.GetComponent<PlayerCameraResizer>().SwapCanvas(true);

            avaliblePlayerInputs[i].actions.FindActionMap("UI").Enable();
            avaliblePlayerInputs[i].actions.FindActionMap("Player").Disable();
        }
    }

    ///<summary>
    /// Swaps all player's control schemes to "Driving" Player movement
    ///</summary>
    public void SwapPlayerControlSchemeToDrive()
    {
        Debug.Log("<color=green>Swapping To Driving Controls</color>");

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
                continue;

            avaliblePlayerInputs[i].gameObject.GetComponent<PlayerCameraResizer>().SwapCanvas(false);

            avaliblePlayerInputs[i].actions.FindActionMap("UI").Disable();
            avaliblePlayerInputs[i].actions.FindActionMap("Player").Enable();
        }
    }

    ///<summary>
    /// Sets the index player to ready
    ///</summary>
    public void ReadyUp(int playerIndexToReadyUp) 
    { 
        playerReadyUp[playerIndexToReadyUp] = true;
        CheckReadyUpCount();
    }

    ///<summary>
    /// Sets the index player to unready
    ///</summary>
    public void UnreadyUp(int playerIndexToReadyUp) 
    { 
        playerReadyUp[playerIndexToReadyUp] = false; 
        if(readyUpCountdown != null)
        {
            PlayerSelectCanvas.Instance.StopCountdown();
            StopCoroutine(readyUpCountdown);
            readyUpCountdown = null;
        }
    }

    ///<summary>
    /// Disables all player's bools of readied up
    ///</summary>
    public void DisableReadiedUp() 
    { 
        isAllReadedUp = false;
        
        for(int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            playerReadyUp[i] = false;
        }
    }

    ///<summary>
    /// Resets player canvas' when loading into scene
    ///</summary>
    public void ResetPlayerCanvas()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
                continue;

            avaliblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().ResetCanvas();
        }
    }

    ///<summary>
    /// Pause was triggered, must pause for all players
    ///</summary>
    public void PlayerPause(PlayerInput playerInput)
    {
        SwapPlayerControlSchemeToUI();

        Time.timeScale = 0f;

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
                continue;

            // For one who paused
            if (playerInput == avaliblePlayerInputs[i])
            {
                avaliblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().hostPause = true;
                avaliblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().pauseMenu.OnPause(PauseMenu.PauseType.Host);
            }
            else
            {
                avaliblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().hostPause = false;
                avaliblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().pauseMenu.OnPause(PauseMenu.PauseType.Sub);
            }
        }
    }

    ///<summary>
    /// Unpause was triggered, must unpause for all players
    ///</summary>
    public void PlayerPlay()
    {
        SwapPlayerControlSchemeToDrive();

        Time.timeScale = 1f;

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
                continue;

            avaliblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().pauseMenu.OnPlay();
        }
    }

    public void PlayerUpdateDrivingIndicators()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (avaliblePlayerInputs[i] == null)
                continue;

            avaliblePlayerInputs[i].gameObject.GetComponentInChildren<DrivingIndicators>().UpdatePlayerReferencesForObjects();
        }
    }

}

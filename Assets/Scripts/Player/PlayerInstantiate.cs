using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInstantiate : SingletonMonobehaviour<PlayerInstantiate>
{
    [SerializeField]
    Rect[] cameraRects;

    [Header("Player Info")]
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject playerHolder;
    [SerializeField] SceneManager sceneManager;

    [Space(10)]
    [Tooltip("Enables or disables the ability for players to spawn into the lobby")]
    [SerializeField] bool allowPlayerSpawn = true;

    [Tooltip("This value is our current player count, ie the # of players in match")]
    [SerializeField] int playerCount = 0;
    public int PlayerCount { get { return playerCount; } }

    [Tooltip("the list of availible player input class objects")]
    [SerializeField] PlayerInput[] availiblePlayerInputs = new PlayerInput[Constants.MAX_PLAYERS];
    public PlayerInput[] PlayerInputs { get { return availiblePlayerInputs; } }
    [Tooltip("The indexed array of player spawn positions")]
    [SerializeField] GameObject[] menuSpawnPositions = new GameObject[Constants.MAX_PLAYERS];

    [Tooltip("The indexed array of player render texutres")]
    [SerializeField] RenderTexture[] playerRenderTextures = new RenderTexture[Constants.MAX_PLAYERS];

    [Header("Company Information")]
    [SerializeField] CompanyInformation[] companies;

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

    [Header("Loading Screen Ready Information")]
    [Tooltip("The indexed array tracking players' loading screen ready status")]
    [SerializeField] bool[] playerLoadingConfirm = new bool[Constants.MAX_PLAYERS];

    [Header("Other")]
    private Gamepad[] playerGamepads = new Gamepad[Constants.MAX_PLAYERS];
    public Gamepad[] PlayerGamepads => playerGamepads;
    CutsceneManager cutsceneManager;
       
    ///<summary>
    /// OnEnable, where i set event methods
    ///</summary>
    public void OnEnable()
    {
        gameManager.OnSwapPlayerSelect += EnablePlayerSpawn;
        gameManager.OnSwapPlayerSelect += SwapForCharacterSelect;

        gameManager.OnSwapLoading += DisablePlayerSpawn;

        gameManager.OnSwapStartingCutscene += PlayerUpdateDrivingIndicators;

        gameManager.OnSwapTutorial += SwapForDriving;

        gameManager.OnSwapStartingCutscene += SwapForCutscene;
        gameManager.OnSwapGoldenCutscene += SwapForCutscene;
        gameManager.OnSwapGoldenCutscene += PlayerUpdateDrivingIndicators;

        gameManager.OnSwapFinalPackage += SwapForDriving;

        gameManager.OnSwapResults += DisableReadiedUp;
        gameManager.OnSwapResults += ResetPlayerCanvas;
        gameManager.OnSwapResults += SwapForResults;
        gameManager.OnSwapResults += SetAllPlayerSpawn;

        gameManager.OnSwapMenu += SwapForMainMenu;

        // When players confirm load, disable all confirm bools
        sceneManager.OnConfirmToLoad += DisableLoadingConfirmCount;
    }

    ///<summary>
    /// OnDisable, where i set event methods
    ///</summary>
    public void OnDisable()
    {
        gameManager.OnSwapPlayerSelect -= EnablePlayerSpawn;
        gameManager.OnSwapPlayerSelect -= SwapForCharacterSelect;

        gameManager.OnSwapLoading -= DisablePlayerSpawn;

        gameManager.OnSwapStartingCutscene -= PlayerUpdateDrivingIndicators;

        gameManager.OnSwapTutorial -= SwapForDriving;

        gameManager.OnSwapStartingCutscene -= SwapForCutscene;
        gameManager.OnSwapGoldenCutscene -= SwapForCutscene;
        gameManager.OnSwapGoldenCutscene -= PlayerUpdateDrivingIndicators;

        gameManager.OnSwapFinalPackage -= SwapForDriving;

        gameManager.OnSwapResults -= DisableReadiedUp;
        gameManager.OnSwapResults -= ResetPlayerCanvas;
        gameManager.OnSwapResults -= SwapForResults;
        gameManager.OnSwapResults -= SetAllPlayerSpawn;

        gameManager.OnSwapMenu -= SwapForMainMenu;

        // When players confirm load, disable all confirm bools
        sceneManager.OnConfirmToLoad -= DisableLoadingConfirmCount;
    }

    ///<summary>
    /// Event to add the player references and rename player
    ///</summary>
    public void AddPlayerReference(PlayerInput playerInput)
    {
        if(playerInput.currentControlScheme != "Gamepad")
        {
            Debug.Log("Hit Mouse or Keyboard");
            Destroy(playerInput.gameObject);
            return;
        }

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

        for(int i = 0; i < availiblePlayerInputs.Length; i++)
        {
            if (availiblePlayerInputs[i] == null)
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
                playerGamepads[0] = playerInput.GetDevice<Gamepad>();
                break;
            case 2:
                ColliderObject.layer = 11; // Player 2;
                ballDriving.playerIndex = 2;
                baseCam.cullingMask |= (1 << 11);
                playerCameraResizer.PlayerRenderCamera.targetTexture = playerRenderTextures[1];
                playerGamepads[1] = playerInput.GetDevice<Gamepad>();
                break;
            case 3:
                ColliderObject.layer = 12; // Player 3;
                ballDriving.playerIndex = 3;
                baseCam.cullingMask |= (1 << 12);
                playerCameraResizer.PlayerRenderCamera.targetTexture = playerRenderTextures[2];
                playerGamepads[2] = playerInput.GetDevice<Gamepad>();
                break;
            case 4:
                ColliderObject.layer = 13; // Player 4;
                ballDriving.playerIndex = 4;
                baseCam.cullingMask |= (1 << 13);
                playerCameraResizer.PlayerRenderCamera.targetTexture = playerRenderTextures[3];
                playerGamepads[3] = playerInput.GetDevice<Gamepad>();
                break;
        }

        playerCameraResizer.InitalizeCompanyScooter(companies[nextFillSlot - 1]);

        // Updates the main virtual camera based on the player number
        playerCameraResizer.UpdateMainVirtualCameras(nextFillSlot);

        // Updates the icon virtual cameras based on the player number
        playerCameraResizer.UpdateIconVirtualCameras(nextFillSlot);

        // Relocates the customization menu based on player number
        playerCameraResizer.RelocateCustomizationMenu(nextFillSlot);

        // Update the naming scheme of the input reciever
        playerInput.gameObject.name = "Player " + nextFillSlot.ToString();
        playerInput.gameObject.transform.parent = playerHolder.transform;
        // assign a company to each player
        playerInput.gameObject.GetComponentInChildren<OrderHandler>().CompanyInfo = companies[nextFillSlot - 1];

        AddToPlayerArray(playerInput);

        // Updates all the player's cameras due to this new player
        UpdatePlayerCameraRects();

        // Swaps the player's control scheme to UI
        SwapPlayerInputControlSchemeToUI();

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
        for (int i = 0; i < availiblePlayerInputs.Length; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;

            // Resets the velocity of the players
            availiblePlayerInputs[i].GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;

            // reset position and rotation of ball and controller
            availiblePlayerInputs[i].GetComponentInChildren<Rigidbody>().transform.position = menuSpawnPositions[i].transform.position;
            availiblePlayerInputs[i].GetComponentInChildren<Rigidbody>().transform.rotation = menuSpawnPositions[i].transform.rotation;

            availiblePlayerInputs[i].GetComponentInChildren<BallDriving>().transform.position = menuSpawnPositions[i].transform.position;
            availiblePlayerInputs[i].GetComponentInChildren<BallDriving>().transform.rotation = menuSpawnPositions[i].transform.rotation;
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

        for (int i = 0; i < availiblePlayerInputs.Length; i++)
        {
            if (availiblePlayerInputs[i] != null)
            {
                if (cameraRectCounter < cameraRects.Length)
                {
                    Debug.Log("Resize Camera");
                    Rect temp = cameraRects[cameraRectCounter];
                    availiblePlayerInputs[i].camera.rect = temp;
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

        ScoreManager.Instance.UpdateOrderHandlers(availiblePlayerInputs);
        QAManager.Instance.UpdateQAHandlers(availiblePlayerInputs);
        
        Destroy(playerInput.gameObject);

        UpdatePlayerCameraRects();

        CheckReadyUpCount();
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
            if (availiblePlayerInputs[i] != null)
            {
                Destroy(availiblePlayerInputs[i].gameObject);
                availiblePlayerInputs[i] = null;
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
            if (availiblePlayerInputs[i] == null)
            {
                availiblePlayerInputs[i] = playerInput;
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
            if (availiblePlayerInputs[i] == playerInput)
            {
                availiblePlayerInputs[i] = null;
                return i;
            }
        }
        return 0;
    }

    ///<summary>
    /// Sets the index player to ready
    ///</summary>
    public void ReadyUp(int playerIndexToReadyUp)
    {
        playerReadyUp[playerIndexToReadyUp] = true;
        availiblePlayerInputs[playerIndexToReadyUp].gameObject.GetComponent<PlayerUIHandler>().customizationSelector.SetDisableOptionsCustomization(true);
        CheckReadyUpCount();
    }

    ///<summary>
    /// Sets the index player to unready
    ///</summary>
    public void UnreadyUp(int playerIndexToReadyUp)
    {
        playerReadyUp[playerIndexToReadyUp] = false;
        availiblePlayerInputs[playerIndexToReadyUp].gameObject.GetComponent<PlayerUIHandler>().customizationSelector.SetDisableOptionsCustomization(false);
        if (readyUpCountdown != null)
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

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            playerReadyUp[i] = false;
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

    ///<summary>
    /// Sets the player index for loading confirm
    ///</summary>
    public void LoadingConfirm(int playerIndexToReadyUp)
    {
        // Does not load confirm platy
        if (sceneManager.EnableConfirm == false)
            return;

        Debug.LogError("CONFIRM LOAD 2");

        LoadingScreenManager.Instance.ConfirmButton(playerIndexToReadyUp);

        playerLoadingConfirm[playerIndexToReadyUp] = true;
        CheckLoadingConfirmCount();
    }

    ///<summary>
    /// Disables all player's bools of readied up
    ///</summary>
    public void DisableLoadingConfirmCount()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            playerLoadingConfirm[i] = false;
        }
    }

    ///<summary>
    /// Checks for how many players are readied up
    ///</summary>
    public void CheckLoadingConfirmCount()
    {
        // Resets and loops to count amount of ready ups
        readyUpCounter = 0;
        foreach (bool readyUpValue in playerLoadingConfirm)
        {
            if (readyUpValue)
                readyUpCounter++;
        }

        // Checks if players are greater then 1 and all players are readied up
        if (readyUpCounter >= playerCount && playerCount >= 1)
        {
            SceneManager.Instance.ConfirmLoad();
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// Swaps the menu control scheme for all players to main menu
    /// </summary>
    private void SwapForMainMenu()
    {
        Debug.Log("Swap for Main Menu");

        SwapPlayerInputControlSchemeToUI();

        SwapMenuTypeForAllPlayers(MenuType.MainMenu);
    }

    /// <summary>
    /// Swaps the menu control scheme for all players to cutscenes
    /// </summary>
    private void SwapForCutscene()
    {
        Debug.Log("Swap for Cutscene");

        SwapPlayerInputControlSchemeToUI();

        SwapMenuTypeForAllPlayers(MenuType.Cutscene);
    }

    /// <summary>
    /// Swaps the menu control scheme for all players to character select
    /// </summary>
    private void SwapForCharacterSelect()
    {
        Debug.Log("Swap for Charater Select");

        SwapPlayerInputControlSchemeToUI();

        SwapMenuTypeForAllPlayers(MenuType.PlayerSelect);
    }

    /// <summary>
    /// Swaps the menu control scheme for all players to results
    /// </summary>
    private void SwapForResults()
    {
        Debug.Log("Swap for Results");

        SwapPlayerInputControlSchemeToUI();

        SwapMenuTypeForAllPlayers(MenuType.ResultsMenu);
    }

    /// <summary>
    /// Swaps the menu type for all players
    /// </summary>
    private void SwapMenuTypeForAllPlayers(MenuType menuType)
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;

            availiblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().menuInteractions.SwapMenuType(menuType);

            // If swapping to pause menu, reparent menu ui to game-camera
            if (menuType == MenuType.PauseMenu)
            {
                availiblePlayerInputs[i].gameObject.GetComponent<PlayerCameraResizer>().ReparentMenuCameraStack(true);
            }
            // If swapping to other menu, reparent menu ui to player-camera on main menu
            else if (menuType == MenuType.MainMenu || menuType == MenuType.ResultsMenu)
            {
                availiblePlayerInputs[i].gameObject.GetComponent<PlayerCameraResizer>().ReparentMenuCameraStack(false);
            }
        }
    }

    ///<summary>
    /// Swaps all player's input control schemes to UI
    ///</summary>
    public void SwapPlayerInputControlSchemeToUI()
    {
        Debug.Log("<color=green>Swapping To UI Controls</color>");

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;

            availiblePlayerInputs[i].gameObject.GetComponent<PlayerCameraResizer>().SwapCanvas(true);

            availiblePlayerInputs[i].actions.FindActionMap("UI").Enable();
            availiblePlayerInputs[i].actions.FindActionMap("Player").Disable();
            availiblePlayerInputs[i].actions.FindActionMap("Load").Disable();
        }
    }

    ///<summary>
    /// Swaps all player's input control schemes to "Driving" Player movement
    ///</summary>
    public void SwapPlayerInputControlSchemeToDrive()
    {
        Debug.Log("<color=green>Swapping To Driving Controls</color>");

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;

            availiblePlayerInputs[i].gameObject.GetComponent<PlayerCameraResizer>().SwapCanvas(false);

            availiblePlayerInputs[i].actions.FindActionMap("UI").Disable();
            availiblePlayerInputs[i].actions.FindActionMap("Player").Enable();
            availiblePlayerInputs[i].actions.FindActionMap("Load").Disable();
        }
    }

    ///<summary>
    /// Swaps all player's input control schemes to "NoInput" Player movement
    ///</summary>
    public void SwapPlayerInputControlSchemeToLoad()
    {
        Debug.Log("<color=green>Swapping To Load Controls</color>");

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;

            availiblePlayerInputs[i].actions.FindActionMap("UI").Disable();
            availiblePlayerInputs[i].actions.FindActionMap("Player").Disable();
            availiblePlayerInputs[i].actions.FindActionMap("Load").Enable();
        }
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

        SwapPlayerInputControlSchemeToDrive();
        SwapMenuTypeForAllPlayers(MenuType.PauseMenu);
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
    /// Resets player canvas' when loading into scene
    ///</summary>
    public void ResetPlayerCanvas()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;

            availiblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().ResetCanvas();
        }
    }

    ///<summary>
    /// Pause was triggered, must pause for all players
    ///</summary>
    public void PlayerPause(PlayerInput playerInput)
    {
        SwapPlayerInputControlSchemeToUI();

        Time.timeScale = 0f;

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;

            // For one who paused
            if (playerInput == availiblePlayerInputs[i])
            {
                availiblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().hostPause = true;
                availiblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().pauseMenu.OnPause(PauseMenu.PauseType.Host);
            }
            else
            {
                availiblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().hostPause = false;
                availiblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().pauseMenu.OnPause(PauseMenu.PauseType.Sub);
            }
        }
    }

    ///<summary>
    /// Unpause was triggered, must unpause for all players
    ///</summary>
    public void PlayerPlay()
    {
        SwapPlayerInputControlSchemeToDrive();

        Time.timeScale = 1f;

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;

            availiblePlayerInputs[i].gameObject.GetComponent<PlayerUIHandler>().MenuCanvas.GetComponent<MenuInteractions>().pauseMenu.OnPlay();
        }
    }

    /// <summary>
    /// Updates driving indicators for all players
    /// </summary>
    public void PlayerUpdateDrivingIndicators()
    {
        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (availiblePlayerInputs[i] == null)
                continue;
            availiblePlayerInputs[i].gameObject.GetComponentInChildren<DrivingIndicators>().UpdatePlayerReferencesForObjects();
        }
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MenuType
{
    MainMenu,
    PlayerSelect,
    Cutscene,
    PauseMenu,
    ResultsMenu
}

public class MenuInteractions : MonoBehaviour
{
    [Header("Bool Information")]
    public bool hostPlayer;
    [SerializeField] bool readiedUp = false;
    public bool hostPause = false;

    [Header("Object References")]
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PlayerUIHandler uiHandler;
    [SerializeField] InputManager drivingHandler;
    [SerializeField] BallDriving ballDriving;

    [Header("UI References")]
    [SerializeField] CustomizationSelector customizationSelector;
    public PauseMenu pauseMenu;
    [SerializeField] MainMenu mainMenu;
    [SerializeField] ResultsMenu resultsMenu;
    [SerializeField] GameObject readyUpText;

    private SoundPool soundPool;

    public MenuType curentMenuType;
    private MenuType cache;

    private void Update()
    {
        if(cache != curentMenuType)
        {
            SwapMenuType(curentMenuType);
        }
    }

    private void OnEnable()
    {
        soundPool = GetComponentInParent<SoundPool>();

        GameManager.Instance.OnSwapPlayerSelect += SwapToPlayerSelect;

        GameManager.Instance.OnSwapStartingCutscene += PlayerUnready;

    }

    private void OnDisable()
    {
        ClearMenuInputs();

        GameManager.Instance.OnSwapPlayerSelect -= SwapToPlayerSelect;

        GameManager.Instance.OnSwapStartingCutscene -= PlayerUnready;
    }

    public void SwapToPlayerSelect()
    {
        SwapMenuType(MenuType.PlayerSelect);
    }

    public void SwapToCutscene()
    {
        SwapMenuType(MenuType.Cutscene);
    }

    public void SwapMenuType(MenuType curentMenuTypePass)
    {
        curentMenuType = curentMenuTypePass;
        cache = curentMenuTypePass;

        ClearMenuInputs();
        switch (curentMenuTypePass)
        {
            case MenuType.MainMenu:
                MainMenuInteractions();
                return;
            case MenuType.PlayerSelect:
                CharacterSelectMenuInteractons();
                return;
            case MenuType.Cutscene:
                Debug.Log("no input for cutscene");
                return;
            case MenuType.PauseMenu:
                PauseMenuInteractons();
                return;
            case MenuType.ResultsMenu:
                ResultsMenuInteractions();
                return;
        }
    }

    private void ClearMenuInputs()
    {
        Debug.Log("<color=blue>Clear Inputs</color>");
        customizationSelector.gameObject.SetActive(false);

        uiHandler.SouthFaceEvent.RemoveAllListeners();
        uiHandler.EastFaceEvent.RemoveAllListeners();

        uiHandler.DownPadEvent.RemoveAllListeners();
        uiHandler.UpPadEvent.RemoveAllListeners();
        uiHandler.RightPadEvent.RemoveAllListeners();
        uiHandler.LeftPadEvent.RemoveAllListeners();

        uiHandler.StartPadEvent.RemoveAllListeners();
        drivingHandler.StartPadEvent.RemoveAllListeners();
    }

    private void MainMenuInteractions()
    {
        // Reference main menu every time we swap
        mainMenu = MainMenu.Instance;

        Debug.Log("<color=blue>Swap to Main Menu</color>");

        uiHandler.DownPadEvent.AddListener(MainMenuScrollDown);
        uiHandler.UpPadEvent.AddListener(MainMenuScrollUp);

        uiHandler.SouthFaceEvent.AddListener(MainMenuConfirm);
    }

    private void CharacterSelectMenuInteractons()
    {
        Debug.Log("<color=blue>Swap to Character Select Menu</color>");

        customizationSelector.gameObject.SetActive(true);

        uiHandler.SouthFaceEvent.AddListener(PlayerReady);
        uiHandler.EastFaceEvent.AddListener(PlayerUnreadyDespawn);

        uiHandler.DownPadEvent.AddListener(CustomizationScrollDown);
        uiHandler.UpPadEvent.AddListener(CustomizationScrollUp);
        uiHandler.RightPadEvent.AddListener(CustomizationScrollRight);
        uiHandler.LeftPadEvent.AddListener(CustomizationScrollLeft);
    }

    private void PauseMenuInteractons()
    {
        Debug.Log("<color=blue>Swap to Pause Menu</color>");

        uiHandler.StartPadEvent.AddListener(PlayGame);
        drivingHandler.StartPadEvent.AddListener(PauseGame);

        uiHandler.DownPadEvent.AddListener(PauseScrollDown);
        uiHandler.UpPadEvent.AddListener(PauseScrollUp);

        uiHandler.SouthFaceEvent.AddListener(PauseConfirm);
    }


    private void ResultsMenuInteractions()
    {
        Debug.Log("<color=blue>Swap to Results Menu</color>");

        // Reference main menu every time we swap
        mainMenu = MainMenu.Instance;

        uiHandler.SouthFaceEvent.AddListener(ResultsMenuConfirm);
    }

    ///<summary>
    /// Calls method when player wants to ready
    ///</summary>
    private void PlayerReady(bool button)
    {
        readyUpText.SetActive(true);
        readiedUp = true;
        PlayerInstantiate.Instance.ReadyUp(ballDriving.playerIndex - 1);
        soundPool.PlayEnterUI();
    }

    ///<summary>
    /// Calls method when player wants to unready or despawn
    ///</summary>
    private void PlayerUnreadyDespawn(bool button)
    {
        // Despawn
        if (readiedUp == false)
        {
            if(hostPlayer == false)
            {
                PlayerInstantiate.Instance.SubtractPlayerCount();
                PlayerInstantiate.Instance.RemovePlayerReference(transform.parent.gameObject.transform.parent.GetComponent<PlayerInput>());
            }
            else
            {
                Debug.Log("You are host, you cannot leave");
            }
        }
        else
        {
            readyUpText.SetActive(false);
            readiedUp = false;
            PlayerInstantiate.Instance.UnreadyUp(ballDriving.playerIndex - 1);
            soundPool.PlayBackUI();
        }
    }

    public void PlayerUnready()
    {
        Debug.Log("Unready");

        readyUpText.SetActive(false);
        readiedUp = false;
        PlayerInstantiate.Instance.UnreadyUp(ballDriving.playerIndex - 1);
    }

    ///<summary>
    /// Calls method when player scrolls up on Main Menu
    ///</summary>
    private void MainMenuScrollUp(bool button)
    {
        if (hostPlayer == false)
            return;

        if (mainMenu == null)
            mainMenu = MainMenu.Instance;

        // Scrolls selector up
        mainMenu.ScrollMenu(false);

        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls down on Main Menu
    ///</summary>
    private void MainMenuScrollDown(bool button)
    {
        if (hostPlayer == false)
            return;

        if (mainMenu == null)
            mainMenu = MainMenu.Instance;

        // Scrolls selector down
        mainMenu.ScrollMenu(true);

        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls down on Main Menu
    ///</summary>
    private void MainMenuConfirm(bool button)
    {
        if (hostPlayer == false)
            return;

        if (mainMenu == null)
            mainMenu = MainMenu.Instance;

        // Scrolls selector down
        mainMenu.ConfirmMenu();

        soundPool.PlayScrollUI();
    }


    ///<summary>
    /// Calls method when player scrolls down on Main Menu
    ///</summary>
    private void ResultsMenuConfirm(bool button)
    {
        if (hostPlayer == false)
            return;

        if (resultsMenu == null)
            resultsMenu = ResultsMenu.Instance;

        // Scrolls selector down
        resultsMenu.ConfirmMenu();

        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls down on character customization
    ///</summary>
    private void CustomizationScrollDown(bool button)
    {
        // Scrolls selector down
        customizationSelector.SetCustomizationType(true);
        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls up on character customization
    ///</summary>
    private void CustomizationScrollUp(bool button)
    {
        // Scrolls selector up
        customizationSelector.SetCustomizationType(false);
        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls left on character customization
    ///</summary>
    private void CustomizationScrollLeft(bool button)
    {
        // Scrolls selector left
        customizationSelector.SetCustomizationValue(false);
        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls right on character customization
    ///</summary>
    private void CustomizationScrollRight(bool button)
    {
        // Scrolls selector right
        customizationSelector.SetCustomizationValue(true);
        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls up on Pause
    ///</summary>
    private void PauseScrollUp(bool button)
    {
        if (hostPause == false)
            return;

        // Scrolls selector up
        pauseMenu.ScrollMenu(false);

        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls down on Pause
    ///</summary>
    private void PauseScrollDown(bool button)
    {
        if (hostPause == false)
            return;

        // Scrolls selector down
        pauseMenu.ScrollMenu(true);

        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method when player scrolls down on Pause
    ///</summary>
    private void PauseConfirm(bool button)
    {
        if (hostPause == false)
            return;

        // Scrolls selector down
        pauseMenu.ConfirmMenu();

        soundPool.PlayScrollUI();
    }

    ///<summary>
    /// Calls method to reset canvas
    ///</summary>
    public void ResetCanvas()
    {
        readyUpText.SetActive(false);
        soundPool.PlayScrollUI();
    }

    public void PlayGame(bool button)
    {
        Debug.Log("Play Game");
        PlayerInstantiate.Instance.PlayerPlay();
    }

    public void PauseGame(bool button)
    {
        Debug.Log("Pause Game Host");
        PlayerInstantiate.Instance.PlayerPause(playerInput);
    }

}

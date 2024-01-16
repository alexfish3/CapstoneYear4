using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuType
{
    MainMenu,
    CharacterSelect,
    PauseMenu,
    ResultsMenu
}

public class MenuInteractions : MonoBehaviour
{
    [SerializeField] PlayerUIHandler uiHandler;
    [SerializeField] InputManager drivingHandler;
    [SerializeField] BallDriving ballDriving;

    public event Action test; 

    [Header("UI References")]
    [SerializeField] CustomizationSelector customizationSelector;
    [SerializeField] PauseMenu pauseMenu;
    [SerializeField] GameObject readyUpText;

    private SoundPool soundPool;

    public MenuType curentMenuType = MenuType.CharacterSelect;

    private void OnEnable()
    {
        soundPool = GetComponentInParent<SoundPool>();

        // Starts the menu on the character select buttons
        SwapMenuType(curentMenuType);
    }

    private void OnDisable()
    {
        ClearMenuInputs();
    }

    public void SwapMenuType(MenuType curentMenuType)
    {
        ClearMenuInputs();
        switch (curentMenuType)
        {
            case MenuType.MainMenu:
                CharacterSelectMenuInteractons();
                return;
            case MenuType.CharacterSelect:
                CharacterSelectMenuInteractons();
                return;
            case MenuType.PauseMenu:
                PauseMenuInteractons();
                return;
            case MenuType.ResultsMenu:
                return;
        }
    }

    private void ClearMenuInputs()
    {
        Debug.Log("<color=blue>Clear Inputs</color>");

        uiHandler.SouthFaceEvent.RemoveAllListeners();
        uiHandler.EastFaceEvent.RemoveAllListeners();

        uiHandler.DownPadEvent.RemoveAllListeners();
        uiHandler.UpPadEvent.RemoveAllListeners();
        uiHandler.RightPadEvent.RemoveAllListeners();
        uiHandler.LeftPadEvent.RemoveAllListeners();

        uiHandler.StartPadEvent.RemoveAllListeners();
        drivingHandler.StartPadEvent.RemoveAllListeners();
    }

    private void CharacterSelectMenuInteractons()
    {
        Debug.Log("<color=blue>Swap to Character Select Menu</color>");

        uiHandler.SouthFaceEvent.AddListener(PlayerReady);
        uiHandler.EastFaceEvent.AddListener(PlayerUnready);

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
    }


    ///<summary>
    /// Calls method when player wants to ready
    ///</summary>
    private void PlayerReady(bool button)
    {
        readyUpText.SetActive(true);
        PlayerInstantiate.Instance.ReadyUp(ballDriving.playerIndex - 1);
        soundPool.PlayEnterUI();
    }

    ///<summary>
    /// Calls method when player wants to unready
    ///</summary>
    private void PlayerUnready(bool button)
    {
        readyUpText.SetActive(false);
        PlayerInstantiate.Instance.UnreadyUp(ballDriving.playerIndex - 1);
        soundPool.PlayBackUI();
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

        pauseMenu.OnPlay();
        PlayerInstantiate.Instance.PlayerPlay();

    }

    public void PauseGame(bool button)
    {
        Debug.Log("Pause Game");
        pauseMenu.OnPause();
        PlayerInstantiate.Instance.PlayerPause();
    }

}

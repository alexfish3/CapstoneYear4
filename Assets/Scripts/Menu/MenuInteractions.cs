using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInteractions : MonoBehaviour
{
    [SerializeField] PlayerUIHandler handler;
    [SerializeField] BallDriving ballDriving;

    [Header("UI References")]
    [SerializeField] CustomizationSelector customizationSelector;
    [SerializeField] GameObject readyUpText;

    private void OnEnable()
    {
        handler.SouthFaceEvent += PlayerReady;
        handler.EastFaceEvent += PlayerUnready;

        handler.DownPadEvent += CustomizationScrollDown;
        handler.UpPadEvent += CustomizationScrollUp;
        handler.RightPadEvent += CustomizationScrollRight;
        handler.LeftPadEvent += CustomizationScrollLeft;
    }

    private void OnDisable()
    {
        handler.SouthFaceEvent -= PlayerReady;
        handler.EastFaceEvent -= PlayerUnready;

        handler.DownPadEvent -= CustomizationScrollDown;
        handler.UpPadEvent -= CustomizationScrollUp;
        handler.RightPadEvent -= CustomizationScrollRight;
        handler.LeftPadEvent -= CustomizationScrollLeft;
    }

    ///<summary>
    /// Calls method when player wants to ready
    ///</summary>
    private void PlayerReady(bool button)
    {
        readyUpText.SetActive(true);
        PlayerInstantiate.Instance.ReadyUp(ballDriving.playerIndex - 1);
    }

    ///<summary>
    /// Calls method when player wants to unready
    ///</summary>
    private void PlayerUnready(bool button)
    {
        readyUpText.SetActive(false);
        PlayerInstantiate.Instance.UnreadyUp(ballDriving.playerIndex - 1);
    }

    ///<summary>
    /// Calls method when player scrolls down on character customization
    ///</summary>
    private void CustomizationScrollDown(bool button)
    {
        // Scrolls selector down
        customizationSelector.SetCustomizationType(true);
    }

    ///<summary>
    /// Calls method when player scrolls up on character customization
    ///</summary>
    private void CustomizationScrollUp(bool button)
    {
        // Scrolls selector up
        customizationSelector.SetCustomizationType(false);
    }

    ///<summary>
    /// Calls method when player scrolls left on character customization
    ///</summary>
    private void CustomizationScrollLeft(bool button)
    {
        // Scrolls selector left
        customizationSelector.SetCustomizationValue(false);
    }

    ///<summary>
    /// Calls method when player scrolls right on character customization
    ///</summary>
    private void CustomizationScrollRight(bool button)
    {
        // Scrolls selector right
        customizationSelector.SetCustomizationValue(true);
    }

    ///<summary>
    /// Calls method to reset canvas
    ///</summary>
    public void ResetCanvas()
    {
        readyUpText.SetActive(false);
    }

}

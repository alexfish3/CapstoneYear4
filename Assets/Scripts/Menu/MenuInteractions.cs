using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInteractions : MonoBehaviour
{
    [SerializeField] PlayerUIHandler handler;
    [SerializeField] BallDriving ballDriving;

    [Header("UI References")]
    [SerializeField] GameObject readyUpText;

    private void OnEnable()
    {
        handler.SouthFaceEvent += PlayerReady;
        handler.EastFaceEvent += PlayerUnready;
    }

    private void OnDisable()
    {
        handler.SouthFaceEvent -= PlayerReady;
        handler.EastFaceEvent -= PlayerUnready;
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
    /// Calls method to reset canvas
    ///</summary>
    public void ResetCanvas()
    {
        readyUpText.SetActive(false);
    }

}

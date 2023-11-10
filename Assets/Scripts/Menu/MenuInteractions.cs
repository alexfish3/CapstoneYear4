using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInteractions : MonoBehaviour
{
    [SerializeField] PlayerUIHandler handler;
    [SerializeField] BallDriving ballDriving;

    private void OnEnable()
    {
        handler.SouthFaceEvent += playerReadyUp;
        handler.EastFaceEvent += playerUnreadyUp;
    }

    private void OnDisable()
    {
        handler.SouthFaceEvent -= playerReadyUp;
        handler.EastFaceEvent -= playerUnreadyUp;
    }

    public void playerReadyUp(bool button)
    {
        PlayerInstantiate.Instance.readyUp(ballDriving.playerIndex - 1);
    }
    public void playerUnreadyUp(bool button)
    {
        PlayerInstantiate.Instance.unreadyUp(ballDriving.playerIndex - 1);
    }

}

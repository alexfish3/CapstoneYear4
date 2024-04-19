using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

///<summary>
/// To be added to any world object that we want to be tracked by compass
///</summary>
public class CompassMarker : MonoBehaviour
{
    public Sprite icon;
    PlayerInstantiate playerInstantiate;

    public void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;
    }

    private void OnEnable()
    {
        //GameManager.Instance.OnSwapResults += RemoveCompassUIFromAllPlayers;
    }

    private void OnDisable()
    {
        //GameManager.Instance.OnSwapResults -= RemoveCompassUIFromAllPlayers;
    }

    ///<summary>
    /// Initalizes the compass ui on all players
    ///</summary>
    public void InitalizeCompassUIOnAllPlayers()
    {
        if(playerInstantiate == null)
            playerInstantiate = PlayerInstantiate.Instance;

        //To be used upon package spawning
        foreach (PlayerInput player in playerInstantiate.PlayerInputs )
        {
            if (player == null)
                continue;

            player.gameObject.GetComponentInChildren<Compass>().AddCompassMarker(this);
        }
    }

    ///<summary>
    /// Removes the icon for all players
    ///</summary>
    public void RemoveCompassUIFromAllPlayers()
    {
        if (playerInstantiate == null)
            playerInstantiate = PlayerInstantiate.Instance;

        for (int i = 0; i < Constants.MAX_PLAYERS; i++)
        {
            if (playerInstantiate.PlayerInputs[i] == null)
                continue;

            PlayerInput player = playerInstantiate.PlayerInputs[i];

            player.gameObject.GetComponentInChildren<Compass>().RemoveCompassMarker(this);
        }
    }

    ///<summary>
    /// Switches the icon on the compass ui
    /// isCarried indicates a scooter icon
    /// !isCarried indicates a floor icon
    ///</summary>
    public void SwitchCompassUIForPlayers(bool isCarried)
    {
        if (playerInstantiate == null)
            playerInstantiate = PlayerInstantiate.Instance;

        //To be used upon package spawning
        foreach (PlayerInput player in playerInstantiate.PlayerInputs)
        {
            if (player == null)
                continue;

            player.gameObject.GetComponentInChildren<Compass>().ChangeCompassMarkerIcon(this, isCarried);
        }
    }
}

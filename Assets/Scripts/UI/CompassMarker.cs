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

    ///<summary>
    /// Initalizes the compass ui on all players
    ///</summary>
    public void InitalizeCompassUIOnAllPlayers()
    {
        if(playerInstantiate == null)
            playerInstantiate = PlayerInstantiate.Instance;

        //To be used upon package spawning
        foreach (PlayerInput player in playerInstantiate.PlayerInputs)
        {
            if (player == null)
                continue;

            player.gameObject.GetComponent<Compass>().AddCompassMarker(this);
        }
    }

    ///<summary>
    /// Removes the icon for all players
    ///</summary>
    public void RemoveCompassUIFromAllPlayers()
    {
        if (playerInstantiate == null)
            playerInstantiate = PlayerInstantiate.Instance;

        //To be used upon package spawning
        foreach (PlayerInput player in playerInstantiate.PlayerInputs)
        {
            if (player == null)
                continue;

            player.gameObject.GetComponent<Compass>().RemoveCompassMarker(this);
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

            player.gameObject.GetComponent<Compass>().ChangeCompassMarkerIcon(this, isCarried);
        }
    }
}

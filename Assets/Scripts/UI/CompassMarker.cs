using UnityEngine;
using UnityEngine.InputSystem;

///<summary>
/// To be added to any world object that we want to be tracked by compass
///</summary>
[RequireComponent(typeof(SphereCollider))]
public class CompassMarker : MonoBehaviour
{
    public Sprite icon;
    PlayerInstantiate playerInstantiate;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameObject playerParent = other.gameObject;

            int indexOfObject = playerParent.GetComponent<Compass>().CompassMarkerObjects.IndexOf(this);

            playerParent.GetComponent<Compass>().CompassUIObjects[indexOfObject].GetComponent<Animator>().SetTrigger("FadeOut");
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameObject playerParent = other.gameObject;

            int indexOfObject = playerParent.GetComponent<Compass>().CompassMarkerObjects.IndexOf(this);

            playerParent.GetComponent<Compass>().CompassUIObjects[indexOfObject].GetComponent<Animator>().SetTrigger("FadeIn");
        }
    }

    public void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;
    }

    public void Update()
    {
        // Adds to player on debug space button
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //foreach (PlayerInput player in playerInstantiate.PlayerInputs)
            //{
            //    if (player == null)
            //        continue;

            //    player.gameObject.GetComponent<Compass>().AddCompassMarker(this);
            //}
        }
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

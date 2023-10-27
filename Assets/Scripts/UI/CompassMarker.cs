using UnityEngine;
using UnityEngine.InputSystem;

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

        // To be used upon package spawning
        //foreach(PlayerInput players in playerInstantiate.PlayerInputs)
        //{
        //    players.gameObject.GetComponent<Compass>().AddCompassMarker(this);
        //}
    }

    public void Update()
    {
        // Adds to player on debug space button
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (PlayerInput player in playerInstantiate.PlayerInputs)
            {
                if (player == null)
                    continue;

                player.gameObject.GetComponent<Compass>().AddCompassMarker(this);
            }
        }
    }
}

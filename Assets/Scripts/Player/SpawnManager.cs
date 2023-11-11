using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : SingletonMonobehaviour<SpawnManager>
{
    GameManager gameManager;
    PlayerInstantiate playerInstantiate;

    [Tooltip("The spawn positions of the players as they start the game")]
    [SerializeField] GameObject[] gameSpawnPositions = new GameObject[Constants.MAX_PLAYERS];
    [SerializeField] GameObject[] goldenPackageSpawnPositions = new GameObject[Constants.MAX_PLAYERS];

    ///<summary>
    /// On Enable of Script
    ///</summary>
    private void OnEnable()
    {
        gameManager = GameManager.Instance;
        playerInstantiate = PlayerInstantiate.Instance;

        Debug.Log("Enable to add spawns");
        gameManager.OnSwapBegin += SpawnPlayersStartOfGame;
        gameManager.OnSwapFinalPackage += SpawnPlayersFinalPackage;
    }

    ///<summary>
    /// On Disable of Script
    ///</summary>
    private void OnDisable()
    {
        gameManager.OnSwapBegin -= SpawnPlayersStartOfGame;
        gameManager.OnSwapFinalPackage -= SpawnPlayersFinalPackage;
    }

    private void Start()
    {
        // Set game to begin upon loading into scene
        gameManager.SetGameState(GameState.Begin);
    }

    ///<summary>
    /// This is executed when the OnSwapBegin event is called
    ///</summary>
    private void SpawnPlayersStartOfGame()
    {
        // Loops for all spawned players
        for (int i = 0; i <= playerInstantiate.PlayerCount - 1; i++)
        {
            // Resets the velocity of the players
            playerInstantiate.PlayerInputs[i].GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;

            // reset position and rotation of ball and controller
            playerInstantiate.PlayerInputs[i].GetComponentInChildren<Rigidbody>().transform.position = gameSpawnPositions[i].transform.position;
            playerInstantiate.PlayerInputs[i].GetComponentInChildren<Rigidbody>().transform.rotation = gameSpawnPositions[i].transform.rotation;

            playerInstantiate.PlayerInputs[i].GetComponentInChildren<BallDriving>().transform.position = gameSpawnPositions[i].transform.position;
            playerInstantiate.PlayerInputs[i].GetComponentInChildren<BallDriving>().transform.rotation = gameSpawnPositions[i].transform.rotation;

            // Initalize the compass ui on each of the players
            playerInstantiate.PlayerInputs[i].gameObject.GetComponentInChildren<CompassMarker>().InitalizeCompassUIOnAllPlayers();
        }

        // After players have been placed, begin main loop
        gameManager.SetGameState(GameState.MainLoop);
    }

    ///<summary>
    /// This is executed when the OnSwapFinalPackage event is called
    ///</summary>
    public void SpawnPlayersFinalPackage()
    {
        // Loops for all spawned players
        for (int i = 0; i <= playerInstantiate.PlayerCount - 1; i++)
        {
            // Resets the velocity of the players
            playerInstantiate.PlayerInputs[i].GetComponentInChildren<Rigidbody>().velocity = Vector3.zero;

            // reset position and rotation of ball and controller
            playerInstantiate.PlayerInputs[i].GetComponentInChildren<Rigidbody>().transform.position = goldenPackageSpawnPositions[i].transform.position;
            playerInstantiate.PlayerInputs[i].GetComponentInChildren<Rigidbody>().transform.rotation = goldenPackageSpawnPositions[i].transform.rotation;

            playerInstantiate.PlayerInputs[i].GetComponentInChildren<BallDriving>().transform.position = goldenPackageSpawnPositions[i].transform.position;
            playerInstantiate.PlayerInputs[i].GetComponentInChildren<BallDriving>().transform.rotation = goldenPackageSpawnPositions[i].transform.rotation;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : SingletonMonobehaviour<SpawnManager>
{
    GameManager gameManager;
    PlayerInstantiate playerInstantiate;

    [SerializeField] GameObject[] gameSpawnPositions = new GameObject[Constants.MAX_PLAYERS];

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

        // Set game to begin upon loading into scene
        gameManager.SetGameState(GameState.Begin);
    }

    ///<summary>
    /// On Disable of Script
    ///</summary>
    private void OnDisable()
    {
        gameManager.OnSwapBegin -= SpawnPlayersStartOfGame;
        gameManager.OnSwapFinalPackage -= SpawnPlayersFinalPackage;
    }

    ///<summary>
    /// This is executed when the OnSwapBegin event is called
    ///</summary>
    private void SpawnPlayersStartOfGame()
    {
        // Loops for all spawned players
        for(int i = 0; i <= playerInstantiate.PlayerCount - 1; i++)
        {
            playerInstantiate.PlayerInputs[i].gameObject.transform.position = gameSpawnPositions[i].transform.position;
            playerInstantiate.PlayerInputs[i].gameObject.transform.rotation = gameSpawnPositions[i].transform.rotation;
        }
    }

    ///<summary>
    /// This is executed when the OnSwapFinalPackage event is called
    ///</summary>
    private void SpawnPlayersFinalPackage()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : SingletonMonobehaviour<TutorialManager>
{
    private bool shouldTutorialize = true;
    private int alumni = 0;

    /// <summary>
    /// Simple counter for checking if every player has completed the tutorial. Will start the game proper if they all have.
    /// </summary>
    /// <param name="amount">Amount to increment by, default is 1</param>
    public void IncrementAlumni(int amount = 1)
    {
        alumni+=amount;
        
        if (alumni < 0)
            alumni = 0;

        Debug.Log($"alumni: {alumni} | PlayerCount: {PlayerInstantiate.Instance.PlayerCount}");
        if(alumni == PlayerInstantiate.Instance.PlayerCount)
        {
            GameManager.Instance.SetGameState(GameState.Begin);
        }
    }
}

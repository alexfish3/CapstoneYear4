using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : SingletonMonobehaviour<TutorialManager>
{
    private bool shouldTutorialize = true;
    public bool ShouldTutorialize { get { return shouldTutorialize; } set { shouldTutorialize = value; } }
    private int alumni = 0;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapTutorial += SkipTutorial;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapTutorial -= SkipTutorial;
    }
    /// <summary>
    /// Simple counter for checking if every player has completed the tutorial. Will start the game proper if they all have.
    /// </summary>
    /// <param name="amount">Amount to increment by, default is 1</param>
    public void IncrementAlumni(int amount = 1)
    {
        alumni+=amount;
        
        if (alumni < 0)
            alumni = 0;

        if(alumni >= PlayerInstantiate.Instance.PlayerCount)
        {
            GameManager.Instance.SetGameState(GameState.Begin);
        }
    }

    /// <summary>
    /// Called when tutorial state is set, skips tutorial if player toggles it.
    /// </summary>
    public void SkipTutorial()
    {
        if (shouldTutorialize)
            return;

        alumni = 0;
        GameManager.Instance.SetGameState(GameState.Begin);
    }
}
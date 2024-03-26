using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialType
{
    Boost,
    Pickup,
    Steal,
    Dropoff
}

public class TutorialManager : SingletonMonobehaviour<TutorialManager>
{
    private bool shouldTutorialize = true;
    public bool ShouldTutorialize { get { return shouldTutorialize; } set { shouldTutorialize = value; } }

    private List<TutorialHandler> handlers = new List<TutorialHandler>();

    public delegate void TutorialComplete();
    public TutorialComplete OnTutorialComplete;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapTutorial += SkipTutorial;
        shouldTutorialize = true;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapTutorial -= SkipTutorial;
    }
    /// <summary>
    /// Adds a tutorial handler to a list if it's not already there.
    /// </summary>
    /// <param name="inHandler">Handler to to be added to the list</param>
    public void IncrementAlumni(TutorialHandler inHandler)
    {
        if(!handlers.Contains(inHandler))
        {
            handlers.Add(inHandler);
        }

        if(handlers.Count >= PlayerInstantiate.Instance.PlayerCount)
        {
            if(shouldTutorialize)
                GameManager.Instance.SetGameState(GameState.Begin);
            
            OnTutorialComplete?.Invoke();
        }
    }

    /// <summary>
    /// Called when tutorial state is set, skips tutorial if player toggles it.
    /// </summary>
    public void SkipTutorial()
    {
        handlers.Clear();

        if (shouldTutorialize)
            return;

        OnTutorialComplete?.Invoke();
        
        GameManager.Instance.SetGameState(GameState.Begin);
    }
}
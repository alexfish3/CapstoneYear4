using DG.Tweening;
using System;
using UnityEngine;

public enum GameState { 
    Menu,
    Options,
    PlayerSelect,
    StartingCutscene,
    Tutorial,
    Begin,
    MainLoop,
    GoldenCutscene,
    FinalPackage,
    Results,
    Paused,
    Default 
}

public class GameManager : SingletonMonobehaviour<GameManager>
{
    [SerializeField] GameState beginingGameState = GameState.PlayerSelect;

    [Space(10)]
    [SerializeField] private GameState mainState = GameState.Default;

    public GameState MainState { get { return mainState; } }

    public event Action OnSwapMenu;
    public event Action OnSwapOptions;
    public event Action OnSwapPlayerSelect;
    public event Action OnSwapStartingCutscene;
    public event Action OnSwapTutorial;
    public event Action OnSwapBegin;
    public event Action OnSwapMainLoop;
    public event Action OnSwapGoldenCutscene;
    public event Action OnSwapFinalPackage;
    public event Action OnSwapResults;
    public event Action OnSwapAnything;

    public void Start()
    {
        SetGameState(beginingGameState);
    }

    ///<summary>
    /// Allows swapping of game states, and also invokes right event when swapping
    ///</summary>
    public void SetGameState(GameState state)
    {
        Debug.Log($"Setting state to: {state}");
        mainState = state;

        if(mainState != GameState.Tutorial)
            OnSwapAnything?.Invoke();

        // Calls an event for the certain state that is swapped to
        switch(mainState)
        {
            case GameState.Menu:
                OnSwapMenu?.Invoke();
                break;
            case GameState.Options:
                OnSwapOptions?.Invoke();
                break;
            case GameState.PlayerSelect:
                OnSwapPlayerSelect?.Invoke();
                break;
            case GameState.StartingCutscene:
                OnSwapStartingCutscene?.Invoke();
                break;
            case GameState.Tutorial:
                OnSwapTutorial?.Invoke();
                break;
            case GameState.Begin:
                OnSwapBegin?.Invoke();
                break;
            case GameState.MainLoop:
                OnSwapMainLoop?.Invoke();
                break;
            case GameState.FinalPackage:
                OnSwapFinalPackage?.Invoke();
                break;
            case GameState.GoldenCutscene:
                OnSwapGoldenCutscene?.Invoke();
                break;
            case GameState.Results:
                DOTween.CompleteAll();
                OnSwapResults?.Invoke();
                break;
            default:
                break;
        }
    }
}

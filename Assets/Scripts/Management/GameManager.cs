using System;
using UnityEngine;

public enum GameState { 
    Menu,
    PlayerSelect,
    StartingCutscene,
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
    public event Action OnSwapPlayerSelect;
    public event Action OnSwapStartingCutscene;
    public event Action OnSwapBegin;
    public event Action OnSwapMainLoop;
    public event Action OnSwapGoldenCutscene;
    public event Action OnSwapFinalPackage;
    public event Action OnSwapResults;

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

        // Calls an event for the certain state that is swapped to
        switch(mainState)
        {
            case GameState.Menu:
                OnSwapMenu?.Invoke();
                break;
            case GameState.PlayerSelect:
                OnSwapPlayerSelect?.Invoke();
                break;
            case GameState.StartingCutscene:
                OnSwapStartingCutscene?.Invoke();
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
                OnSwapResults?.Invoke();
                break;
            default:
                break;
        }
    }
}

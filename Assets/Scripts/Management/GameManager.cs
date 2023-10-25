using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum GameState { Menu, PlayerSelect, Begin, Main, FinalPackage, Results }

public class GameManager : SingletonMonobehaviour<GameManager>
{
    private GameState mainState = GameState.Menu;

    public event Action OnSwapMenu;
    public event Action OnSwapPlayerSelect;
    public event Action OnSwapBegin;
    public event Action OnSwapMain;
    public event Action OnSwapFinalPackage;
    public event Action OnSwapResults;

    ///<summary>
    /// Allows swapping of game states, and also invokes right event when swapping
    ///</summary>
    public void SetGameState(GameState state)
    {
        mainState = state;

        Debug.Log($"Setting state to: {state}");

        switch(mainState)
        {
            case GameState.Menu:
                OnSwapMenu?.Invoke();
                break;
            case GameState.PlayerSelect:
                OnSwapPlayerSelect?.Invoke();
                break;
            case GameState.Begin:
                OnSwapBegin?.Invoke();
                break;
            case GameState.Main:
                OnSwapMain?.Invoke();
                break;
            case GameState.FinalPackage:
                OnSwapFinalPackage?.Invoke();
                break;
            case GameState.Results:
                OnSwapResults?.Invoke();
                break;
            default:
                break;
        }

    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotKeys : SingletonMonobehaviour<HotKeys>
{
    public event Action onIncrementWave;
    public event Action onDecrementWave;
    private void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameManager.Instance.SetGameState(GameState.Menu);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameManager.Instance.SetGameState(GameState.PlayerSelect);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            GameManager.Instance.SetGameState(GameState.Begin);
        }*/
        if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            GameManager.Instance.SetGameState(GameState.MainLoop);
        }
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            GameManager.Instance.SetGameState(GameState.FinalPackage);
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            onIncrementWave?.Invoke();
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            onDecrementWave?.Invoke();
        }
        /*if(Input.GetKeyDown(KeyCode.Alpha6))
        {
            GameManager.Instance.SetGameState(GameState.Results);
        }*/
    }
}

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
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GameManager.Instance.SetGameState(GameState.Begin);
        }
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            GameManager.Instance.SetGameState(GameState.FinalPackage);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            GameManager.Instance.SetGameState(GameState.Results);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            onIncrementWave?.Invoke();
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            onDecrementWave?.Invoke();
        }
    }
}

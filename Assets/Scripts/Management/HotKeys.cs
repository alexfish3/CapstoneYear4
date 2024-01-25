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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            onDecrementWave?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            onIncrementWave?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (GameManager.Instance.MainState != GameState.Menu && GameManager.Instance.MainState != GameState.PlayerSelect)
            {
                GameManager.Instance.SetGameState(GameState.StartingCutscene);
            }
        }
        if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (GameManager.Instance.MainState != GameState.Menu && GameManager.Instance.MainState != GameState.PlayerSelect)
            {
                GameManager.Instance.SetGameState(GameState.GoldenCutscene);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (GameManager.Instance.MainState != GameState.Menu && GameManager.Instance.MainState != GameState.PlayerSelect)
            {
                GameManager.Instance.SetGameState(GameState.Results);
            }
        }
    }
}

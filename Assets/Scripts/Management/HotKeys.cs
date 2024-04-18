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
        if (Input.GetKeyDown(KeyCode.Alpha1)) // decrease wave
        {
            onDecrementWave?.Invoke();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2)) // increase wave
        {
            onIncrementWave?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.Instance.LoadGameScene();
        }
    }
}

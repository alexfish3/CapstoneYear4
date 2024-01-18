using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSelectCanvas : SingletonMonobehaviour<PlayerSelectCanvas>
{
    [Header("Countdown")]
    [SerializeField] TMP_Text countdownText;

    public void UpdateCountdown(float countdown)
    {
        countdownText.text = countdown.ToString();
    }

    public void StopCountdown()
    {
        countdownText.text = "";
    }

}

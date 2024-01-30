using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSelectCanvas : SingletonMonobehaviour<PlayerSelectCanvas>
{
    [Header("Text Information")]
    [SerializeField] TMP_Text countdownText;
    [SerializeField] GameObject[] pressButtonTexts;

    [Header("Other")]
    [SerializeField] PlayerInstantiate playerInstantiate;

    public void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;

        TogglePressButtonOnAllTexts(playerInstantiate.PlayerInputs);
    }

    public void UpdateCountdown(float countdown)
    {
        countdownText.text = countdown.ToString();
    }

    public void StopCountdown()
    {
        countdownText.text = "";
    }

    // Toggles the press button text on one player's position
    public void TogglePressButtonTexts(int position, bool toggleOnOff)
    {
        pressButtonTexts[position].SetActive(toggleOnOff);
    }

    // Toggles the press button text on all player's positions that are in-game
    public void TogglePressButtonOnAllTexts(PlayerInput[] playerInputs)
    {
        for(int i = 0; i < playerInputs.Length; i++)
        {
            if (playerInputs[i] == null)
                continue;

            TogglePressButtonTexts(i, false);
        }
    }

}

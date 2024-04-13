using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// This is a testing class to update the score text on the player's UI for the Package Scene. This is all the commenting I'm going to bother to do for this class and it already took me longer to write this comment than to write the script.
/// </summary>
public class DynamicNumberUI : MonoBehaviour
{
    [SerializeField] private NumberHandler numHandler;
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] TextMeshProUGUI centerText;

    // final order countdown
    [SerializeField] TextMeshProUGUI finalOrderText;
    private int currFinal = -1;

    TimeSpan timeSpan;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapAnything += SetNothing;
        GameManager.Instance.OnSwapMenu += () => finalOrderText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapAnything -= SetNothing;
        GameManager.Instance.OnSwapMenu -= () => finalOrderText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the UI for normal gameplay.
    /// </summary>
    public void SetNormalGameplay()
    {
        centerText.text = "";
        timeSpan = TimeSpan.FromSeconds(OrderManager.Instance.GameTimer);
        waveText.text = timeSpan.ToString("m\\:ss\\.ff");
        finalOrderText.gameObject.SetActive(false);
        finalOrderText.text = "";
    }

    /// <summary>
    /// Sets the UI for the final order area.
    /// </summary>
    public void SetFinalGameplay()
    {
        waveText.text = "";
        waveText.color = Color.white;
        centerText.text = "";
        finalOrderText.gameObject.SetActive(true);
        finalOrderText.text = $": ${OrderManager.Instance.FinalOrderValue}";
    }

    /// <summary>
    /// Sets the UI for the countdown between main game and final area.
    /// </summary>
    /// <param name="inTime"></param>
    public void SetFinalCountdown(int inTime)
    {
        numHandler.SetFinalCountdown(true);
        waveText.text = "";
        finalOrderText.text = "";
        if (currFinal != inTime)
        {
            currFinal = inTime;
            numHandler.UpdateFinalCountdown(inTime);
        }
    }

    /// <summary>
    /// Sets all text to nothing.
    /// </summary>
    public void SetNothing()
    {
        numHandler.SetFinalCountdown(false);
        waveText.text = "";
        finalOrderText.text = "";
        centerText.text = "";
    }

    private void Update()
    {
        if (OrderManager.Instance != null)
        {
            if (OrderManager.Instance.GameStarted)
            {
                if (!OrderManager.Instance.FinalOrderActive)
                {
                    if ((int)OrderManager.Instance.GameTimer < 10 && (int)OrderManager.Instance.GameTimer > 0)
                    {
                        SetFinalCountdown((int)OrderManager.Instance.GameTimer);
                    }
                    else
                    {
                        SetNormalGameplay();
                    }
                }
                else
                {
                    if (GameManager.Instance.MainState == GameState.FinalPackage)
                        SetFinalGameplay();
                    else
                        SetNothing();
                }
            }
        }
    }
}

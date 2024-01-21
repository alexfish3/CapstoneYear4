using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultsMenu : SingletonMonobehaviour<ResultsMenu>
{
    [SerializeField] private TMP_Text[] displayText;
    [SerializeField] Canvas resultsCanvas;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapResults += UpdateResults;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapResults -= UpdateResults;
    }

    private void UpdateResults()
    {
        resultsCanvas.enabled = true;
        for (int i = 0; i < displayText.Length; i++)
        {
            OrderHandler currHandler = ScoreManager.Instance.GetHandlerOfIndex(i);
            if (currHandler != null)
            {
                displayText[i].text = currHandler.Placement + ". " + currHandler.gameObject.transform.parent.name + " | $" + currHandler.Score;
            }
        }
    }

    public void ConfirmMenu()
    {
        GameManager.Instance.SetGameState(GameState.Menu);
    }
}

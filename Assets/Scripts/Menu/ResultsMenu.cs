using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultsMenu : SingletonMonobehaviour<ResultsMenu>
{
    [SerializeField] private TMP_Text[] displayText;
    [SerializeField] Canvas resultsCanvas;

    [SerializeField] private RawImage[] playerImages;

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
        for (int i = 0; i < PlayerInstantiate.Instance.PlayerCount; i++)
        {
            OrderHandler currHandler = ScoreManager.Instance.GetHandlerOfIndex(i);
            if (currHandler != null)
            {
                playerImages[i].enabled = true;
                playerImages[i].texture = currHandler.CompanyInfo.playerTexture; //readd when the eotm menu is working
                //displayText[i].text = currHandler.Placement + ". " + currHandler.gameObject.transform.parent.name + " | $" + currHandler.Score;
            }
        }
    }

    public void ConfirmMenu()
    {
        foreach(RawImage img in playerImages)
        {
            img.enabled = false;
        }

        SceneManager.Instance.InvokeMenuSceneEvent();
        //GameManager.Instance.SetGameState(GameState.Menu);
    }
}

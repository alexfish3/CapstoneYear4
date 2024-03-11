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
    [SerializeField] private GameObject quitInfo;
    bool canQuit = false;

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
        quitInfo.SetActive(false);
        resultsCanvas.enabled = true;
        StartCoroutine(QuitDelay());
        for (int i = 0; i < PlayerInstantiate.Instance.PlayerCount; i++)
        {
            OrderHandler currHandler = ScoreManager.Instance.GetHandlerOfIndex(i);
            if (currHandler != null)
            {
                playerImages[i].enabled = true;
                playerImages[i].texture = currHandler.CompanyInfo.playerTexture; //readd when the eotm menu is working

                displayText[i].enabled = true;
                displayText[i].text = "$" + currHandler.Score;
            }
        }
    }

    public void ConfirmMenu()
    {
        if(!canQuit)
        {
            return;
        }
        SceneManager.Instance.InvokeMenuSceneEvent();

        for (int i=0;i<displayText.Length;i++)
        {
            playerImages[i].enabled = false;
            displayText[i].enabled = false;
        }

        //GameManager.Instance.SetGameState(GameState.Menu);
    }

    private IEnumerator QuitDelay()
    {
        canQuit = false;
        yield return new WaitForSeconds(2f);
        quitInfo.SetActive(true);
        canQuit = true;
    }
}

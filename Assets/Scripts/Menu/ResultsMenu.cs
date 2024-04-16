using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResultsMenu : SingletonMonobehaviour<ResultsMenu>
{
    [SerializeField] private TMP_Text[] displayText;
    [SerializeField] Canvas resultsCanvas;

    [SerializeField] private GameObject quitInfo;

    [SerializeField] private Camera cam;
    bool canQuit = false;

    [SerializeField] private GameObject wiper;
    [SerializeField] private GameObject canvasElements;
    private Animator transitionAnimator;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapResults += UpdateResults;
        GameManager.Instance.OnFinalOrderDelivered += HideGame;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapResults -= UpdateResults;
        GameManager.Instance.OnFinalOrderDelivered -= HideGame;
    }

    private void Start()
    {
        cam.enabled = false;
        transitionAnimator = wiper.GetComponent<Animator>();
    }

    private void HideGame()
    {
        canvasElements.SetActive(false);
        resultsCanvas.enabled = true;
        wiper.SetActive(true);
        transitionAnimator.SetTrigger("WipeIn");
    }

    private void UpdateResults()
    {
        Debug.Log("update results");
        canvasElements.SetActive(true);
        transitionAnimator.SetTrigger("WipeOut");
        quitInfo.SetActive(false);
        resultsCanvas.enabled = true;
        cam.enabled = true;
        StartCoroutine(QuitDelay());

        for (int i = 0; i < PlayerInstantiate.Instance.PlayerCount; i++)
        {
            OrderHandler currHandler = ScoreManager.Instance.GetHandlerOfIndex(i);

            // Set player animations
            Animator playerAnim = currHandler.transform.parent.GetComponent<PlayerCameraResizer>().playerAnimator;
            playerAnim.SetInteger("End Status", i + 1);

            if (currHandler != null)
            {
                displayText[i].enabled = true;
                displayText[i].text = "$" + currHandler.Score;
            }
        }
    }

    public void ConfirmMenu()
    {
        if (!canQuit)
        {
            return;
        }
        SceneManager.Instance.InvokeMenuSceneEvent();

        for (int i = 0; i < displayText.Length; i++)
        {
            displayText[i].enabled = false;
        }

        // Reset player result animations
        for (int i = 0; i < PlayerInstantiate.Instance.PlayerCount; i++)
        {
            OrderHandler currHandler = ScoreManager.Instance.GetHandlerOfIndex(i);
            Animator playerAnim = currHandler.transform.parent.GetComponent<PlayerCameraResizer>().playerAnimator;
            playerAnim.SetInteger("End Status", 0);
        }
    }

    private IEnumerator QuitDelay()
    {
        canQuit = false;
        yield return new WaitForSeconds(2f);
        quitInfo.SetActive(true);
        canQuit = true;
    }
}

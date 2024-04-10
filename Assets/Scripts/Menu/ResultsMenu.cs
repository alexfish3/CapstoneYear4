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

    [SerializeField] private Camera orthoCam;
    bool canQuit = false;

    RectTransform canvasRect;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapResults += UpdateResults;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapResults -= UpdateResults;
    }

    private void Start()
    {
        canvasRect = resultsCanvas.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            ScaleOrthoCam(orthoCam);
        }
    }

    private void UpdateResults()
    {
        ScaleOrthoCam(orthoCam);
        quitInfo.SetActive(false);
        resultsCanvas.enabled = true;
        StartCoroutine(QuitDelay());

        for (int i = 0; i < PlayerInstantiate.Instance.PlayerCount; i++)
        {
            OrderHandler currHandler = ScoreManager.Instance.GetHandlerOfIndex(i);

            // Set player animations
            Animator playerAnim = currHandler.transform.parent.GetComponent<PlayerCameraResizer>().playerAnimator;
            playerAnim.SetInteger("End Status", i + 1);

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

        // Reset player result animations
        for (int i = 0; i < PlayerInstantiate.Instance.PlayerCount; i++)
        {
            OrderHandler currHandler = ScoreManager.Instance.GetHandlerOfIndex(i);
            Animator playerAnim = currHandler.transform.parent.GetComponent<PlayerCameraResizer>().playerAnimator;
            playerAnim.SetInteger("End Status", 0);
        }
    }

    private void ScaleOrthoCam(Camera cam)
    {
        float upp = 8.8f / Screen.width;
        float desiredOS = 0.5f * upp * Screen.height;
        cam.orthographicSize = desiredOS;
        Debug.Log($"ORTHO LOGIC: 0.5 x {upp} x {Screen.height} = {cam.orthographicSize}");
    }

    private IEnumerator QuitDelay()
    {
        canQuit = false;
        yield return new WaitForSeconds(2f);
        quitInfo.SetActive(true);
        canQuit = true;
    }
}

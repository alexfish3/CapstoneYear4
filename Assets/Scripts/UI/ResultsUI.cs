using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] displayText;
    [SerializeField] private Button returnButton;
    private void Start()
    {
        for(int i = 0; i < displayText.Length; i++)
        {
            OrderHandler currHandler = ScoreManager.Instance.GetHandlerOfIndex(i);
            if(currHandler != null)
            {
                displayText[i].text = currHandler.Placement + ". " + currHandler.gameObject.transform.parent.name + " $" + currHandler.Score;
            }
        }
        returnButton.onClick.AddListener(ResetGame);
    }

    private void ResetGame()
    {
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        GameManager.Instance.SetGameState(GameState.Begin);
    }
}

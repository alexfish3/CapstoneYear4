using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : SingletonMonobehaviour<MainMenu>
{
    [Header("Selector Objects")]
    [SerializeField] GameObject selector;
    [SerializeField] GameObject[] selectorObjects;
    int selectorPos;

    [Header("Player Select Objects")]
    [SerializeField] Canvas PlayerSelectCanvas;

    public void ScrollMenu(bool direction)
    {
        // Positive Scroll
        if (direction)
        {
            if (selectorPos == selectorObjects.Length - 1)
            {
                selectorPos = 0;
            }
            else
            {
                selectorPos = selectorPos + 1;
            }
        }
        // Negative Scroll
        else
        {
            if (selectorPos == 0)
            {
                selectorPos = selectorObjects.Length - 1;
            }
            else
            {
                selectorPos = selectorPos - 1;
            }
        }

        // Updates selector for current slider selected
        selector.transform.position = new Vector3(selector.transform.position.x, selectorObjects[selectorPos].transform.position.y, selector.transform.position.z);
    }

    public void ConfirmMenu()
    {
        switch (selectorPos)
        {
            // Play
            case 0:
                Debug.Log("Play");
                SwapToPlayerSelect();
                break;
            // Quit
            case 1:
                Debug.Log("Quit");
                break;
        }
    }

    public void SwapToPlayerSelect()
    {
        PlayerSelectCanvas.enabled = true;

        GameManager.Instance.SetGameState(GameState.PlayerSelect);
    }
}

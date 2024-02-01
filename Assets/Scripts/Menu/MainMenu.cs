using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MainMenu : SingletonMonobehaviour<MainMenu>
{
    [Header("Selector Objects")]
    [SerializeField] GameObject selector;
    [SerializeField] GameObject[] selectorObjects;
    int selectorPos;

    [Header("Player Select Objects")]
    [SerializeField] Canvas PlayerSelectCanvas;

    [SerializeField] TMP_Text p1ConnectedController;
    PlayerInstantiate playerInstantiate;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (playerInstantiate == null)
                playerInstantiate = PlayerInstantiate.Instance;

            playerInstantiate.ClearPlayerArray();
            ScoreManager.Instance.UpdateOrderHandlers(playerInstantiate.PlayerInputs);
            QAManager.Instance.UpdateQAHandlers(playerInstantiate.PlayerInputs);

            p1ConnectedController.text = "";

        }
    }

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

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
                Application.Quit();
                break;
        }
    }

    public void SwapToPlayerSelect()
    {
        PlayerSelectCanvas.enabled = true;

        GameManager.Instance.SetGameState(GameState.PlayerSelect);
    }

    public void SwapToMainMenu()
    {
        PlayerSelectCanvas.enabled = false;

        GameManager.Instance.SetGameState(GameState.Menu);
    }

    public void Player1ControllerConnected(PlayerInput playerInput)
    {
        p1ConnectedController.text = playerInput.devices[0].name;
    }
}

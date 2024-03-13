using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LoadingScreenManager : SingletonMonobehaviour<LoadingScreenManager>
{
    [Header("Loading Screen Information")]
    [SerializeField] Transform[] buttonPositions;

    [SerializeField] GameObject[] ButtonGameobjects;
    [SerializeField] GameObject[] ButtonColors;

    ///<summary>
    /// Initalizes the button objects based on player count
    ///</summary>
    public void InitalizeButtonGameobjects(PlayerInput[] playerInputs)
    {
        int buttons = 0;

        // Loops for all spawned players
        for (int i = 0; i < playerInputs.Length; i++)
        {
            if (playerInputs[i] == null)
                continue;

            ButtonGameobjects[i].gameObject.transform.position = buttonPositions[buttons].transform.position;

            // Enables buttons required for players
            ButtonGameobjects[i].gameObject.SetActive(true);

            buttons++;
        }
    }

    ///<summary>
    /// Disables all buttons after they are not needed
    ///</summary>
    public IEnumerator DisableButtonGameobjects()
    {
        yield return new WaitForSeconds(1f);

        // Set Buttons to false
        for (int i = 0; i < 4; i++)
        {
            ButtonGameobjects[i].gameObject.SetActive(false);
            ButtonColors[i].gameObject.SetActive(false);
        }
    }

    ///<summary>
    /// Confirms button for one player
    ///</summary>
    public void ConfirmButton(int playerPos)
    {
        ButtonColors[playerPos].gameObject.SetActive(true);
    }

}

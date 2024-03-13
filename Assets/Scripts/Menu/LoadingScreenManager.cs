using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenManager : SingletonMonobehaviour<LoadingScreenManager>
{
    [Header("Loading Screen Information")]
    [SerializeField] GameObject[] ButtonGameobjects;
    [SerializeField] GameObject[] ButtonColors;

    ///<summary>
    /// Initalizes the button objects based on player count
    ///</summary>
    public void InitalizeButtonGameobjects(int playerCount)
    {
        // Enables buttons required for players
        for (int i = 0; i < playerCount; i++)
        {
            ButtonGameobjects[i].gameObject.SetActive(true);
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

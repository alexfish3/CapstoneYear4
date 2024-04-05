using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : SingletonMonobehaviour<CreditsMenu>
{
    [SerializeField] MainMenu menu;

    public void UpdateSelectors()
    {

    }

    ///<summary>
    /// Exits to the main menu
    ///</summary>
    public void ExitMenu()
    {
        Debug.Log("Exit credits");

        menu.SwapToMainMenu();
    }
}

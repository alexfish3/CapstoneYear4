using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : SingletonMonobehaviour<CreditsMenu>
{
    [SerializeField] MainMenu menu;
    [SerializeField] Animator animator;

    public void BeginCredits()
    {
        animator.SetTrigger("ScrollCredits");
    }

    ///<summary>
    /// Exits to the main menu
    ///</summary>
    public void ExitMenu()
    {
        Debug.Log("Exit credits");

        animator.SetTrigger("Reset");
        menu.SwapToMainMenu();
    }
}

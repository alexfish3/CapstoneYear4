using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : SingletonMonobehaviour<CreditsMenu>
{
    [SerializeField] MainMenu menu;
    [SerializeField] Animator animator;

    public void BeginCredits()
    {
        animator.SetTrigger(HashReference._scrollCreditsTrigger);
    }

    ///<summary>
    /// Exits to the main menu
    ///</summary>
    public void ExitMenu()
    {
        animator.SetTrigger(HashReference._resetTrigger);
        menu.SwapToMainMenu();
    }
}

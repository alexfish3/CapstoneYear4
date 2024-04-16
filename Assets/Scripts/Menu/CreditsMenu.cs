using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenu : SingletonMonobehaviour<CreditsMenu>
{
    [SerializeField] GameObject returnButton;
    [SerializeField] MainMenu menu;
    [SerializeField] Animator animator;
    bool finished = false;

    public void Update()
    {
        // Calls when the credits finished
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9 && animator.GetCurrentAnimatorStateInfo(0).IsName("Credit_Anim") && finished == false)
        {
            finished = true;
            returnButton.SetActive(true);
        }
    }

    public void BeginCredits()
    {
        returnButton.SetActive(false);
        finished = false;
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

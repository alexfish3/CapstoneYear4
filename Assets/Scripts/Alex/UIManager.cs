using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : SingletonMonobehaviour<UIManager>
{
    [SerializeField] TMP_Text player1Text;
    [SerializeField] TMP_Text player2Text;

    [SerializeField] TMP_Text boostCounter1;
    [SerializeField] TMP_Text boostCounter2;

    [SerializeField] TMP_Text canBoost1;
    [SerializeField] TMP_Text canBoost2;

    public void updatePlayer1Text(float score)
    {
        player1Text.text = "Score: " + score.ToString();
    }

    public void updatePlayer1Boost(float boost)
    {
        boostCounter1.text = "Boosts: " + boost.ToString();
    }

    public void updatePlayer2Text(float score)
    {
        player2Text.text = "Score: " + score.ToString();
    }

    public void updatePlayer2Boost(float boost)
    {
        boostCounter2.text = "Boosts: " + boost.ToString();
    }

    public void updatePlayer1CanBoost(bool canBoost)
    {
        if (canBoost)
        {
            canBoost1.text = "Can Boost";
        }
        else
        {
            canBoost1.text = "Can Not Boost";
        }
    }

    public void updatePlayer2CanBoost(bool canBoost)
    {
        if (canBoost)
        {
            canBoost2.text = "Can Boost";
        }
        else
        {
            canBoost2.text = "Can Not Boost";
        }
    }
}

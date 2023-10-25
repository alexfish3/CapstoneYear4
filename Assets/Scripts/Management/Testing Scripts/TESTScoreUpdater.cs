using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// This is a testing class to update the score text on the player's UI for the Package Scene. This is all the commenting I'm going to bother to do for this class and it already took me longer to write this comment than to write the script.
/// </summary>
public class TESTScoreUpdater : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    private OrderHandler player;

    private void Start()
    {
        player = GetComponent<OrderHandler>();
    }
    private void Update()
    {
        scoreText.text = "$" + player.Score;
    }
}

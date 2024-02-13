using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialHandler : MonoBehaviour
{
    private bool hasLearnt = false;

    public delegate void LearntDelegate();
    public LearntDelegate OnTutorialized;

    private BallDriving ball;

    [Tooltip("Boost modifier when the player is being tutorialized.")]
    [SerializeField] private float tutorialBoostMod = 10;
    [Tooltip("Text shown on the driving canvas when the player is being tutorialized.")]
    [SerializeField] private TextMeshProUGUI tutorialText;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapStartingCutscene += ResetHandler;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnSwapStartingCutscene -= ResetHandler;
    }

    private void Start()
    {
        ball = GetComponent<BallDriving>();
    }

    /// <summary>
    /// Resets the tutorial handler so players can be tutorialized on subsequent playthroughs.
    /// </summary>
    public void ResetHandler()
    {
        if (TutorialManager.Instance.ShouldTutorialize)
        {
            hasLearnt = false;
            TutorialManager.Instance.IncrementAlumni(-1);
            ball.HardCodeBoostModifier(tutorialBoostMod);
            tutorialText.text = "Press A to Boost";
        }
        else
        {
            ball.SetBoostModifier(false);
            tutorialText.text = "";
        }
    }

    /// <summary>
    /// For when the handler completes the tutorial (boosts through the wall).
    /// </summary>
    public void TeachHandler()
    {
        if (hasLearnt)
            return;

        hasLearnt = true;
        ball.SetBoostModifier(false);
        TutorialManager.Instance.IncrementAlumni();
        tutorialText.text = "";
    }
}

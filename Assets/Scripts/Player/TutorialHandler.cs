using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    private bool hasLearnt = false;

    public delegate void LearntDelegate();
    public LearntDelegate OnTutorialized;

    private BallDriving ball;

    [Tooltip("Boost modifier when the player is being tutorialized")]
    [SerializeField] private float tutorialBoostMod = 10;

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
        ball.HardCodeBoostModifier(tutorialBoostMod);
    }

    /// <summary>
    /// Resets the tutorial handler so players can be tutorialized on subsequent playthroughs.
    /// </summary>
    public void ResetHandler()
    {
        hasLearnt = false;
        TutorialManager.Instance.IncrementAlumni(-1);
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
    }
}

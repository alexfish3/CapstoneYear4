using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    // have enum here
    [Tooltip("The next tutorial in the sequence.")]
    [SerializeField] private TutorialType nextTutorial;


    [Tooltip("Reference to the ground to be disabled once tutorial is complete.")]
    [SerializeField] private GameObject tutorialGround;

    private void Start()
    {
        tutorialGround.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        TutorialHandler handler = other.transform.parent.GetComponentInChildren<TutorialHandler>();
        if (handler != null)
        {
            handler.TeachHandler(nextTutorial); // pass through tutorial type
        }
    }
}

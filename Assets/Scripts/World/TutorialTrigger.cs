using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    // have enum here
    [Tooltip("The next tutorial in the sequence.")]
    [SerializeField] private TutorialType nextTutorial;

    private void OnTriggerEnter(Collider other)
    {
        TutorialHandler handler = other.transform.parent.GetComponentInChildren<TutorialHandler>();
        if (handler != null)
        {
            handler.TeachHandler(nextTutorial); // pass through tutorial type
        }
    }
}

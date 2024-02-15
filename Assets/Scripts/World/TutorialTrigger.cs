using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    [Tooltip("Reference to the ground to be disabled once tutorial is complete.")]
    [SerializeField] private GameObject tutorialGround;

    private IEnumerator disableRoutine;

    private void OnEnable()
    {
        TutorialManager.Instance.OnTutorialComplete += DisableTutorialGround;
    }
    private void OnDisable()
    {
        TutorialManager.Instance.OnTutorialComplete -= DisableTutorialGround;
    }
    private void Start()
    {
        tutorialGround.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        TutorialHandler handler = other.transform.parent.GetComponentInChildren<TutorialHandler>();
        if(handler != null)
        {
            handler.TeachHandler();
        }
    }

    private void DisableTutorialGround()
    {
        if(disableRoutine == null)
        {
            disableRoutine = DisableCooldown();
            StartCoroutine(disableRoutine);
        }
    }

    private IEnumerator DisableCooldown()
    {
        yield return new WaitForSeconds(1f);
        tutorialGround.SetActive(false);
        disableRoutine = null;
    }
}

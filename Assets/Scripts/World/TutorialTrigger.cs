using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    [Tooltip("Corresponding platform.")]
    [SerializeField] private GameObject platform;

    private void Start()
    {
        platform.SetActive(true);
    }
    private void OnTriggerEnter(Collider other)
    {
        TutorialHandler handler = other.transform.parent.GetComponentInChildren<TutorialHandler>();
        if(handler != null)
        {
            handler.TeachHandler();
            platform.SetActive(false);
        }
    }
}

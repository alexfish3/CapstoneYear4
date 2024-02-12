using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        TutorialHandler handler = other.transform.parent.GetComponentInChildren<TutorialHandler>();
        if(handler != null)
        {
            handler.TeachHandler();
        }
    }
}

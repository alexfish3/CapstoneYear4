using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows for little physics objects (cans) to be knocked around when driven into (kicked)
/// </summary>
public class CanKicker : MonoBehaviour
{
    [Tooltip("Reference to the can kicker transform (used for creating a trajectory)")]
    [SerializeField] private Transform canKickingSpot;

    private void OnCollisionEnter(Collision collision)
    {
        
    }
}

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
    [Tooltip("Force to kick the can with")]
    [SerializeField] private float kickingForce = 75;
    [Tooltip("Reference to this player's BallDriving script")]
    [SerializeField] private BallDriving control;

    private Collider sphereCol;

    private void Start()
    {
        sphereCol = control.Sphere.GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Kickable")
        {
            Physics.IgnoreCollision(sphereCol, other);

            Vector3 kickDirection = (other.transform.position - canKickingSpot.position).normalized;
            other.GetComponent<Rigidbody>().AddForce(kickDirection * kickingForce * control.CurrentVelocity);

            other.gameObject.GetComponent<Kickable>().GetKicked(sphereCol);

            PeterSparker.Instance.CreateImpactFromCollider(other, sphereCol.transform.position);
        }
    }
}

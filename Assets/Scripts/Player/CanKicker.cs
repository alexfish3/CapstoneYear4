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
        Kickable kickable = other.GetComponent<Kickable>();

        if (other.tag == "Kickable" && !kickable.Kicked)
        {
            DoKick(other);            
            kickable.GetKicked(sphereCol);
        }
    }

    public void DoKick(Collider col, float kickingModifier = 1.0f, Vector3? overridePosition = null)
    {
        Vector3 kickDirection = (col.transform.position - canKickingSpot.position).normalized;
        if (overridePosition != null)
        {
            kickDirection = ((overridePosition ?? col.transform.position) - canKickingSpot.position).normalized;
        }

        PeterSparker.Instance.CreateImpactFromCollider(col, sphereCol.transform.position);

        col.GetComponent<Rigidbody>().AddForce(kickDirection * kickingForce * control.CurrentVelocity);
    }
}

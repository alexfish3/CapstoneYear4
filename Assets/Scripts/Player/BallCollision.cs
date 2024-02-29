using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class deals with collisions on the Ball of Fun and calls various methods from the player control scripts.
/// </summary>
public class BallCollision : MonoBehaviour
{
    [Tooltip("Reference to the ball driving component")]
    [SerializeField] private BallDriving control;

    private void OnTriggerEnter(Collider other)
    {
        if (control.CurrentVelocity < 10) return;
        if (other.tag == "Kickable" || other.tag == "Speed" || other.tag == "TouchGrass" || other.tag == "Water" || other.tag == "Pickup") return;
        if (other.gameObject.GetComponent<Respawn>() != null) return;
        if (other.gameObject.GetComponent<BallDriving>() != null) return;

        control.BounceOff(other.ClosestPoint(transform.position), 1000 + (20 * control.CurrentVelocity));
        Debug.Log("Bounce with " + (1000 + (20 * control.CurrentVelocity)));
    }
}

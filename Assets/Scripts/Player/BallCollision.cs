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
        if (other.tag == "Kickable" || other.tag == "Speed" || other.tag == "TouchGrass" || other.tag == "Water" || other.tag == "Pickup" || other.tag == "IgnoreBallCollision") return;
        if (other.gameObject.GetComponent<Respawn>() != null) return; //collectively this and the line below ensure its not a player being touched, since that's handled separately
        if (other.gameObject.GetComponent<BallDriving>() != null) return;

        control.DriftDrop(true);

        PeterSparker.Instance.CreateImpactFromCollider(other, control.transform.position);
    }
}

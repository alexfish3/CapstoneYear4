using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkideeSkidoo : MonoBehaviour
{
    [SerializeField] private TrailRenderer frontTire;
    [SerializeField] private TrailRenderer backTire;

    private BallDriving control;

    private void Start()
    {
        control = GetComponent<BallDriving>();

        frontTire.alignment = LineAlignment.TransformZ;
        backTire.alignment = LineAlignment.TransformZ;

        frontTire.emitting = false;
        backTire.emitting = false;
    }

    private void Update()
    {
        frontTire.emitting = control.Grounded && control.Drifting;
        backTire.emitting = control.Grounded && control.Drifting;
    }
}

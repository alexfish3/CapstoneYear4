using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the main class driving the movement and control of the delivery scooters. 
/// It handles acceleration, motion, steering, drifting, and portions of the boosting mechanic.
/// </summary>
public class VehicleControl : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("An input manager class, from the correlated InputReceiver object")]
    [SerializeField] private InputManager inp;

    [Header("Constants")]
    [Tooltip("A constant multiplier which affects how quickly the bike can accelerate")]
    [SerializeField] private float accelerationPower = 10;
    [Tooltip("A constant representing the drag force on the bike; higher values will lead to slower acceleration and faster deceleration")]
    [SerializeField] private float dragForce = 5;
    [Tooltip("A constant multiplier which affects how quickly the bike can brake")]
    [SerializeField] private float brakingPower = 15;

    private float leftStick;
    private float rightTrig;
    private float leftTrig;

    private float deltaSpeed = 0;
    private float speed = 0;


    /// <summary>
    /// Standard Update
    /// Consistantly gets the current state of input from the InputManager and stores it in local variables
    /// Drives most of the action
    /// </summary>
    void Update()
    {
        leftStick = inp.LeftStickValue;
        rightTrig = inp.RightTriggerValue;
        leftTrig = inp.LeftTriggerValue;

        Accelerate();
        Move();
    }

    /// <summary>
    /// Updates the speed variable based on the triggers, and scales for framerate.
    /// </summary>
    void Accelerate()
    {
        float currentSpeed = speed;

        float acceleration = rightTrig * accelerationPower; //accelerationpower is just a scalar since rightTrig is between 0 and 1
        float drag = (currentSpeed * currentSpeed) * dragForce; //drag increases with the square of speed, letting it function as a max
        float braking = leftTrig * brakingPower; //same with brakingpower

        deltaSpeed = acceleration - drag - braking;
        speed += deltaSpeed * Time.deltaTime;

        speed = speed < 0 ? 0 : speed;
    }

    /// <summary>
    /// A primitive forward movement script, which will be adapted once steering is implemented.
    /// </summary>
    private void Move()
    {
        transform.position += Vector3.forward * speed * Time.deltaTime;
    }
}

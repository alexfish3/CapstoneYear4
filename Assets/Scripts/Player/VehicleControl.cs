using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the main class driving the movement and control of the delivery scooters. 
/// It handles acceleration, motion, steering, drifting, and portions of the boosting mechanic.
/// </summary>
public class VehicleControl : MonoBehaviour
{
    private const float STOP_THRESHOLD = 0.5f;

    [Header("Setup")]
    [Tooltip("An input manager class, from the correlated InputReceiver object")]
    [SerializeField] private InputManager inp;

    [Header("Speed Modifiers")]
    [Tooltip("A constant multiplier which affects how quickly the bike can accelerate")]
    [SerializeField] private float accelerationPower = 10;
    [Tooltip("A constant representing the drag force on the bike; higher values will lead to slower acceleration and faster deceleration")]
    [SerializeField] private float dragForce = 5;
    [Tooltip("A constant multipler applied to the dragforce when reversing to give it a lower max speed")]
    [SerializeField] private float reverseDrag = 3;
    [Tooltip("A constant multiplier which affects how quickly the bike can brake")]
    [SerializeField] private float brakingPower = 15;

    [Header("Steering")]
    [Tooltip("The max amount that the bike can turn at once (the amount it turns if the stick is held fully left or right) measured in degrees")]
    [SerializeField] private float maxTurningAngle = 50.0f;
    [Tooltip("A transform of the relative position of the front wheel")]
    [SerializeField] private Transform frontWheelLocation;
    [Tooltip("A transform of the relative position of the back wheel")]
    [SerializeField] private Transform backWheelLocation;

    [Header("Debug")]
    [SerializeField] private bool drawNewDirection = false;

    private float leftStick;
    private float rightTrig;
    private float leftTrig;

    private float deltaSpeed = 0;
    private float speed = 0;

    private Quaternion frontWheelFacing;
    private Vector3 convertedFacing;

    private Rigidbody rb;


    void Start()
    {
            rb = GetComponent<Rigidbody>();
    }

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
        Steer();
    }

    void FixedUpdate()
    {
        rb.velocity = convertedFacing * speed * Time.fixedDeltaTime;

        AlterFacing();
    }

    /// <summary>
    /// Updates the speed variable based on the triggers, and scales for framerate.
    /// </summary>
    private void Accelerate()
    {
        float currentSpeed = speed;

        float acceleration = rightTrig * accelerationPower; //accelerationpower is just a scalar since rightTrig is between 0 and 1
        float drag = (currentSpeed * currentSpeed) * dragForce; //drag increases with the square of speed, letting it function as a max
        float braking = leftTrig * brakingPower; //same with brakingpower

        if (currentSpeed <= 0) //checks whether the player is trying to reverse
        {
            drag *= reverseDrag;

            deltaSpeed = -(braking - drag - acceleration);
        }
        else
        {
            deltaSpeed = acceleration - drag - braking;
        }

        speed += deltaSpeed * Time.deltaTime;
        
        if (acceleration <= 0 && braking <= 0)
        {
            speed = Mathf.Abs(speed) < STOP_THRESHOLD ? 0 : speed; //clamps the speed to 0 when it's already very low; allows for reaching a complete stop, and prevents the bike from very slowly inching forward at low speeds
        }
    }

    /// <summary>
    /// Picks the direction for the scooter to move based on the position of the left stick
    /// Stores it as a Quaternion in frontWheelFacing then as a Vector3 in convertedFacing
    /// </summary>
    private void Steer()
    {
        float targetAngle = leftStick * maxTurningAngle; //ranges from -maxTurningAngle to +maxTurningAngle
        frontWheelFacing = Quaternion.AngleAxis(targetAngle, Vector3.up);

        convertedFacing = frontWheelFacing * transform.forward;
    }

    /// <summary>
    /// A primitive forward movement script, which will be adapted once steering is implemented.
    /// </summary>
    private void AlterFacing()
    {
        Vector3 backWheelStartPos = backWheelLocation.position;

        transform.position += convertedFacing * speed * Time.deltaTime;

        Vector3 newDirection = frontWheelLocation.position - backWheelStartPos;
        newDirection.Normalize();

        if (speed == 0)
        {
            newDirection = Vector3.forward;
        }

        if (drawNewDirection)
        {
            Debug.DrawLine(transform.position, newDirection * 5);
        }

        if (speed != 0)
        {
            rb.MoveRotation(Quaternion.LookRotation(newDirection, transform.up));
        }
    }
}

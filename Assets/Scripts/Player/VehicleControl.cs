using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the main class driving the movement and control of the delivery scooters. 
/// It handles acceleration, motion, steering, drifting, and portions of the boosting mechanic.
/// </summary>
public class VehicleControl : MonoBehaviour
{
    private const float STOP_THRESHOLD = 1.0f;

    private IEnumerator boostTimeCoroutine;
    private IEnumerator boostRechargeCoroutine;

    [Header("Setup")]
    [Tooltip("An input manager class, from the correlated InputReceiver object")]
    [SerializeField] private InputManager inp;
    public InputManager Inp { set { inp = value; } }

    [Header("Speed Modifiers")]
    [Tooltip("A constant multiplier which affects how quickly the bike can accelerate")]
    [SerializeField] private float accelerationPower = 10.0f;
    [Tooltip("A constant representing the drag force on the bike; higher values will lead to slower acceleration and faster deceleration")]
    [SerializeField] private float dragForce = 5.0f;
    [Tooltip("A constant multipler applied to the dragforce when reversing to give it a lower max speed")]
    [SerializeField] private float reverseDrag = 3.0f;
    [Tooltip("A constant multiplier which affects how quickly the bike can brake")]
    [SerializeField] private float brakingPower = 15.0f;
    [Tooltip("An amount directly added to the speed when completing a successful drift")]
    [SerializeField] private float driftBoost = 5.0f;

    [Header("Steering")]
    [Tooltip("The max amount that the bike can turn at once (the amount it turns if the stick is held fully left or right) measured in degrees")]
    [SerializeField] private float maxTurningAngle = 50.0f;
    [Tooltip("A transform of the relative position of the front wheel")]
    [SerializeField] private Transform frontWheelLocation;
    [Tooltip("A transform of the relative position of the back wheel")]
    [SerializeField] private Transform backWheelLocation;

    [Header("Boosting")]
    [Tooltip("The amount of speed added when boosting")]
    [SerializeField] private float boostAmount = 10.0f;
    [Tooltip("How long the boost lasts")]
    [SerializeField] private float boostDuration = 1.0f;
    [Tooltip("How long it takes to recharge the boost, starting after it finishes")]
    [SerializeField] private float boostRechargeTime = 10.0f;

    [Header("Debug")]
    [SerializeField] private bool drawNewDirection = false;

    private float leftStick;
    private float rightTrig;
    private float leftTrig;

    private float deltaSpeed = 0;
    private float speed = 0;

    private bool drifting = false;
    private bool driftDirection; //false for left, true for right
    private float driftTime = 0.0f;

    private bool boostAvailable = true;
    private bool boostActive = false;

    private Quaternion frontWheelFacing;
    private Vector3 convertedFacing;
    Vector3 backWheelStartPos;

    private Rigidbody rb;

    private OrderHandler orderHandler;

    /// <summary>
    /// Standard Start
    /// Just gets references and subscribes to events
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inp.WestFaceEvent += DriftUpdate;
        inp.SouthFaceEvent += PrimitiveBoost;

        orderHandler = GetComponent<OrderHandler>();
    }

    /// <summary>
    /// Standard Update
    /// Consistantly gets the current state of input from the InputManager and stores it in local variables
    /// Drives most of the action
    /// </summary>
    void Update()
    {
        orderHandler.IsBoosting = boostActive;

        leftStick = inp.LeftStickValue;
        rightTrig = inp.RightTriggerValue;
        leftTrig = inp.LeftTriggerValue;

        Accelerate();
        Steer();
        AlterFacing();
    }

    /// <summary>
    /// Standard FixedUpdate
    /// Changes the velocity based on the calculated intended speed, then calls for the scooter to rotate based on steering.
    /// </summary>
    void FixedUpdate()
    {
        if (Mathf.Approximately(Time.fixedDeltaTime, 0f))
            return;
        //rb.velocity.x = (convertedFacing.normalized * speed * 100 * Time.fixedDeltaTime).x;
        rb.AddForce(convertedFacing.normalized * speed * 10);
        backWheelStartPos = backWheelLocation.position;
    }

    /// <summary>
    /// Updates the speed variable based on the triggers, and scales for framerate.
    /// </summary>
    private void Accelerate()
    {
        float currentSpeed = speed;

        float acceleration = rightTrig * accelerationPower; //accelerationpower is just a scalar since rightTrig is between 0 and 1
        float drag = (currentSpeed * currentSpeed) * dragForce; //drag increases with the square of speed, letting it function as a max
        if (boostActive)
        {
            drag *= 0.25f;
        }
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
        if (drifting)
        {
            targetAngle *= 1.5f;
        }
        frontWheelFacing = Quaternion.AngleAxis(targetAngle, Vector3.up);

        convertedFacing = frontWheelFacing * transform.forward;
    }

    /// <summary>
    /// This script takes the desired facing direction determined by Steer and translates that into actual rotation.
    /// It should be called once, at the end of FixedUpdate.
    /// </summary>
    private void AlterFacing()
    {
        //transform.position += convertedFacing * speed * Time.deltaTime;

        Vector3 newDirection = frontWheelLocation.position - backWheelStartPos;
        newDirection.y = 0;
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

    /// <summary>
    /// Simple method to update the drifting status based on an event from InputManager
    /// </summary>
    /// <param name="westFaceState">The current state of the west face button, passed by the event</param>
    private void DriftUpdate(bool westFaceState)
    {
        drifting = westFaceState;
    }

    private void PrimitiveBoost(bool southFaceState)
    {
        if (boostAvailable)
        {
            boostAvailable = false;
            StartBoostTime();
        }
    }

    private IEnumerator BoostTime()
    {
        speed += boostAmount;
        boostActive = true;
        yield return new WaitForSeconds(boostDuration);
        boostActive = false;
        StartBoostRecharge();
    }

    private IEnumerator BoostRecharge()
    {
        yield return new WaitForSeconds(boostRechargeTime);
        boostAvailable = true;
    }

    private void StartBoostTime()
    {
        boostTimeCoroutine = BoostTime();
        StartCoroutine(boostTimeCoroutine);
    }
    
    private void StopBoostTime()
    {
        StopCoroutine(boostTimeCoroutine);
        boostTimeCoroutine = null;
    }

    private void StartBoostRecharge()
    {
        boostRechargeCoroutine = BoostRecharge();
        StartCoroutine(boostRechargeCoroutine);
    }

    private void StopBoostRecharge()
    {
        StopCoroutine(boostRechargeCoroutine);
        boostRechargeCoroutine = null;
    }
}

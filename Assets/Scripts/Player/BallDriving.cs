using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Version 3.0 of the vehicle controller. Drives by rolling a sphere collider around the world then simply matching the bike model to its position
/// </summary>
public class BallDriving : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Reference to the scooter model (specifically whatever empty is right above the first object with any actual mesh)")]
    [SerializeField] private Transform scooterModel;
    [Tooltip("Reference to the empty used for checking the scooter's normal to the ground")]
    [SerializeField] private Transform scooterNormal;
    [Tooltip("Reference to the movement sphere")]
    [SerializeField] private GameObject sphere;
    [Tooltip("An input manager class, from the correlated InputReceiver object")]
    [SerializeField] private InputManager inp;
    public InputManager Inp { set { inp = value; } }

    [Header("Speed Modifiers")]
    [Tooltip("A constant multiplier which affects how quickly the bike can accelerate")]
    [SerializeField] private float accelerationPower = 30.0f;
    [Tooltip("A constant multiplier which affects how quickly the bike can brake and reverse")]
    [SerializeField] private float brakingPower = 10.0f;
    [Tooltip("The amount of speed granted by a successful drift")]
    [SerializeField] private float driftBoost = 5.0f;

    [Header("Steering")]
    [Tooltip("The 'turning power'. A slightly abstract concept representing how well the scooter can turn. Higher values represent a tighter turning circle")]
    [SerializeField] private float steeringPower = 15.0f;

    [Header("Boosting")]
    [Tooltip("The speed power of the boost")]
    [SerializeField] private float boostPower = 50.0f;
    [Tooltip("How long the boost lasts")]
    [SerializeField] private float boostDuration = 1.0f;
    [Tooltip("How long it takes to recharge the boost, starting after it finishes")]
    [SerializeField] private float boostRechargeTime = 10.0f;

    [Header("Debug")]
    [Tooltip("Display debug speedometer")]
    [SerializeField] private bool debugSpeedometerEnable = false;
    [Tooltip("Reference to the TMP for displaying the speed")]
    [SerializeField] private TextMeshProUGUI debugSpeedText;

    private Rigidbody sphereBody; //just reference to components of the sphere
    private Transform sphereTransform;

    private float leftStick; //left stick value, ranging from -1 to 1
    private float leftTrig; //left trigger value, ranging from 0 to 1
    private float rightTrig; //right trigger value, ranging from 0 to 1

    private float currentForce; //the amount of force to add to the speed on any given frame
    private float rotationAmount; //the amount to turn on any given frame


    /// <summary>
    /// Standard Start. Just used to get references and subscribe to events
    /// </summary>
    private void Start()
    {
        sphereBody = sphere.GetComponent<Rigidbody>();
        sphereTransform = sphere.GetComponent<Transform>();
    }

    /// <summary>
    /// Standard Update. Gets controls and updates variables
    /// </summary>
    private void Update()
    {
        leftStick = inp.LeftStickValue;
        leftTrig = inp.LeftTriggerValue;
        rightTrig = inp.RightTriggerValue;

        if (debugSpeedometerEnable)
        {
            debugSpeedText.text = "" + sphereBody.velocity.magnitude;
        }

        transform.position = sphere.transform.position - new Vector3(0, 1, 0); //makes the scooter follow the sphere

        currentForce = (accelerationPower * rightTrig) - (brakingPower * leftTrig); //accelerating, braking, and reversing all in one! Oh my!

        rotationAmount = leftStick * steeringPower;
        rotationAmount *= RangeMutations.Map_SpeedToSteering(currentForce, accelerationPower);
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + rotationAmount, 0), Time.deltaTime);
    }

    /// <summary>
    /// Standard FixedUpdate. Handles the actual movement.
    /// </summary>
    private void FixedUpdate()
    {
        sphereBody.AddForce(transform.forward * currentForce, ForceMode.Acceleration);

        //Clamping to make it easier to come to a complete stop
        if (sphereBody.velocity.magnitude < 1 && currentForce < 1)
        {
            sphereBody.velocity = Vector3.zero;
        }
    }
}

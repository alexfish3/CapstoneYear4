using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private float steeringPower = 80.0f;

    [Header("Boosting")]
    [Tooltip("The speed power of the boost")]
    [SerializeField] private float boostPower = 50.0f;
    [Tooltip("How long the boost lasts")]
    [SerializeField] private float boostDuration = 1.0f;
    [Tooltip("How long it takes to recharge the boost, starting after it finishes")]
    [SerializeField] private float boostRechargeTime = 10.0f;

    private Rigidbody sphereBody;
    private Transform sphereTransform;

    private float leftStick;
    private float leftTrig;
    private float rightTrig;

    private float currentForce;


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

        transform.position = sphere.transform.position - new Vector3(0, 1, 0); //makes the scooter follow the sphere

        currentForce = (accelerationPower * rightTrig) - (brakingPower * leftTrig);
    }

    /// <summary>
    /// Standard FixedUpdate. Handles the actual movement.
    /// </summary>
    private void FixedUpdate()
    {
        sphereBody.AddForce(transform.forward * currentForce, ForceMode.Acceleration);
    }
}

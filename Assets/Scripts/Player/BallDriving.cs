using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Version 3.0 of the vehicle controller. Drives by rolling a sphere collider around the world then simply matching the bike model to its position.
/// Drifting is slightly more complicated, and involves doing a bunch of math.
/// </summary>
public class BallDriving : MonoBehaviour
{
    private const float STEERING_MODEL_ROTATION = 15.0f; //How far the model rotates when steering normally
    private const float DRIFTING_MODEL_ROTATION = 20.0f; //How far the model rotates when drifting
    private const float MODEL_ROTATION_TIME = 0.2f; //How long it takes the model to rotate into full position

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
    [Tooltip("The multiplier applied to turning when drifting. Always above 1 or there'll be no difference. Use caution when messing with this")]
    [SerializeField] private float driftTurnScalar = 1.8f;
    [Tooltip("How many 'drift points' are needed to achieve the drift boost. This is a semi-arbitrary unit, though if the drift boost is being based entirely on time, 100 drift points equals 1 second")]
    [SerializeField] private float driftBoostThreshold = 100.0f;
    [Tooltip("How much time vs turning amount is factored into drift boost. 0 is full time, 1 is full turning amount")]
    [SerializeField] private float driftBoostMode = 0.0f;

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
    [Tooltip("Display debug drift state")]
    [SerializeField] private bool debugDriftStateEnable = false;
    [Tooltip("Reference to the TMP for displaying the speed")]
    [SerializeField] private TextMeshProUGUI debugDriftStateText;
    [Tooltip("Display debug drift completion")]
    [SerializeField] private bool debugDriftCompleteEnable = false;
    [Tooltip("Reference to the image for debug drift completion")]
    [SerializeField] private Image debugDriftComplete;

    private Rigidbody sphereBody; //just reference to components of the sphere
    private Transform sphereTransform;

    private float leftStick; //left stick value, ranging from -1 to 1
    private float leftTrig; //left trigger value, ranging from 0 to 1
    private float rightTrig; //right trigger value, ranging from 0 to 1

    private float currentForce; //the amount of force to add to the speed on any given frame
    private float rotationAmount; //the amount to turn on any given frame

    private bool callToDrift = false;
    private bool drifting = false;
    private int driftDirection;

    private bool driftBoostAchieved = false;
    private float driftPoints = 0.0f;


    /// <summary>
    /// Standard Start. Just used to get references and subscribe to events
    /// </summary>
    private void Start()
    {
        sphereBody = sphere.GetComponent<Rigidbody>();
        sphereTransform = sphere.GetComponent<Transform>();

        inp.WestFaceEvent += DriftFlag; //subscribes to WestFaceEvent
    }

    /// <summary>
    /// Standard Update. Gets controls and updates variables
    /// </summary>
    private void Update()
    {
        leftStick = inp.LeftStickValue;
        leftTrig = inp.LeftTriggerValue;
        rightTrig = inp.RightTriggerValue;

        if (callToDrift && leftStick != 0)
        {
            AssignDriftState();
        }

        transform.position = sphere.transform.position - new Vector3(0, 1, 0); //makes the scooter follow the sphere

        currentForce = (accelerationPower * rightTrig) - (brakingPower * leftTrig); //accelerating, braking, and reversing all in one! Oh my!

        if (drifting)
        {
            rotationAmount = Drift(); //determines the actual rotation of the larger object

            //Rotates just the model; purely for effect
            float driftTargetAmount = (driftDirection > 0) ? RangeMutations.Map_Linear(leftStick, -1, 1, 0.5f, driftTurnScalar) : RangeMutations.Map_Linear(leftStick, -1, 1, driftTurnScalar, 0.5f);
            float modelRotateAmount = 90 + driftTargetAmount * driftDirection * DRIFTING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(currentForce, accelerationPower);
            scooterModel.localEulerAngles = Vector3.Lerp(scooterModel.localEulerAngles, new Vector3(0, modelRotateAmount, scooterModel.localEulerAngles.z), 0.2f);
        }
        else
        {
            //determines the actual rotation of the larger object
            rotationAmount = leftStick * steeringPower;
            rotationAmount *= RangeMutations.Map_SpeedToSteering(currentForce, accelerationPower); //scales steering by speed, though only when not drifting

            //Rotates just the model; purely for effect
            float modelRotateAmount = 90 + leftStick * STEERING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(currentForce, accelerationPower);
            scooterModel.localEulerAngles = Vector3.Lerp(scooterModel.localEulerAngles, new Vector3(0, modelRotateAmount, scooterModel.localEulerAngles.z), 0.2f);
        }

        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + rotationAmount, 0), Time.deltaTime);

        DebugUIUpdate();
    }

    /// <summary>
    /// Standard FixedUpdate. Handles the actual movement.
    /// </summary>
    private void FixedUpdate()
    {
        float totalForce = currentForce;

        if (driftBoostAchieved)
        {
            totalForce += driftBoost;
            driftBoostAchieved = false;
        }

        sphereBody.AddForce(transform.forward * totalForce, ForceMode.Acceleration);        

        //Clamping to make it easier to come to a complete stop
        if (sphereBody.velocity.magnitude < 1 && currentForce < 1)
        {
            sphereBody.velocity = new Vector3(0, sphereBody.velocity.y, 0);
        }
    }

    /// <summary>
    /// Receives input as an event. Flags callToDrift and drifting depending on circumstance. Applies the boost if applicable
    /// </summary>
    /// <param name="WestFaceState">The state of the west face button, passed by the event</param>
    private void DriftFlag(bool WestFaceState)
    {
        if (WestFaceState)
        {
            if (leftStick != 0)
            {
                AssignDriftState();
            }
            else
            {
                callToDrift = true; //can't drift without a selected direction, so it stows the request until a direction is selected
            }
        }
        else
        {
            callToDrift = false;
            drifting = false;

            if (driftPoints >= driftBoostThreshold)
            {
                driftBoostAchieved = true;
                driftPoints = 0.0f;
            }
        }
    }

    /// <summary>
    /// Sets drifting to true and assings its direction based on the left stick.
    /// </summary>
    private void AssignDriftState()
    {
        callToDrift = false;
        drifting = true;

        driftDirection = leftStick < 0? -1 : 1;
    }

    /// <summary>
    /// The function that actually handles drift. 
    /// Scales the user input to be from 0 to driftTurnScalar instead of -1 to 1, enhancing the turning circle but restricting its direction
    /// </summary>
    /// <returns>A rotation amount</returns>
    private float Drift()
    {
        float scaledInput;

        if (driftDirection > 0)
        {
            scaledInput = RangeMutations.Map_Linear(leftStick, -1, 1, 0, driftTurnScalar);
        }
        else
        {
            scaledInput = RangeMutations.Map_Linear(leftStick, -1, 1, driftTurnScalar, 0);
        }

        driftPoints += Time.deltaTime * (1 - driftBoostMode) * 100.0f;
        return steeringPower * driftDirection * scaledInput * RangeMutations.Map_SpeedToSteering(currentForce, accelerationPower);
    }

    /// <summary>
    /// Updates various debug UI elements
    /// </summary>
    private void DebugUIUpdate()
    {
        if (debugSpeedometerEnable)
        {
            debugSpeedText.text = "" + sphereBody.velocity.magnitude;
        }

        if (debugDriftStateEnable)
        {
            debugDriftStateText.text = "Drifting: " + (drifting ? "Yes" : "No");
        }

        if (debugDriftCompleteEnable)
        {
            if (driftPoints >= driftBoostThreshold)
            {
                debugDriftComplete.enabled = true;
            }
            else
            {
                debugDriftComplete.enabled = false;
            }
        }
    }
}

using System.Collections;
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

    private IEnumerator boostActiveCoroutine;
    private IEnumerator boostCooldownCoroutine;

    [Header("Setup")]
    [Tooltip("Reference to the scooter model (specifically whatever empty is right above the first object with any actual mesh)")]
    [SerializeField] private Transform scooterModel;
    [Tooltip("Reference to the empty used for checking the scooter's normal to the ground")]
    [SerializeField] private Transform scooterNormal;
    [Tooltip("Reference to the movement sphere")]
    [SerializeField] private GameObject sphere;
    [Tooltip("An input manager class, from the correlated InputReceiver object")]
    [SerializeField] private InputManager inp;

    [Header("Speed Modifiers")]
    [Tooltip("An amorphous representation of how quickly the bike can accelerate")]
    [SerializeField] private float accelerationPower = 30.0f;
    [Tooltip("An amorphous representation of how hard the bike can brake")]
    [SerializeField] private float brakingPower = 30.0f;
    [Tooltip("An amorphous representation of how quickly the bike can reverse")]
    [SerializeField] private float reversingPower = 10.0f;

    [Header("Steering")]
    [Tooltip("The 'turning power'. A slightly abstract concept representing how well the scooter can turn. Higher values represent a tighter turning circle")]
    [SerializeField] private float steeringPower = 15.0f;
    [Tooltip("The 'turning power' when reversing.")]
    [SerializeField] private float reverseSteeringPower = 40.0f;
    [Tooltip("The multiplier applied to turning when drifting. Always above 1 or there'll be no difference. Use caution when messing with this")]
    [SerializeField] private float driftTurnScalar = 1.8f;
    [Tooltip("How many 'drift points' are needed to achieve the drift boost. This is a semi-arbitrary unit, though if the drift boost is being based entirely on time, 100 drift points equals 1 second")]
    [SerializeField] private float driftBoostThreshold = 100.0f;
    [Tooltip("How much time vs turning amount is factored into drift boost. 0 is full time, 1 is full turning amount")]
    [SerializeField] private float driftBoostMode = 0.0f;
    [Tooltip("The amount of speed granted by a successful drift")]
    [SerializeField] private float driftBoost = 5.0f;

    [Header("Boosting")]
    [Tooltip("The speed power of the boost")]
    [SerializeField] private float boostPower = 50.0f;
    [Tooltip("How long the boost lasts")]
    [SerializeField] private float boostDuration = 1.0f;
    [Tooltip("How long it takes to recharge the boost, starting after it finishes")]
    [SerializeField] private float boostRechargeTime = 10.0f;
    [Tooltip("The amount of drag while boosting (Exercise caution when changing this; ask Will before playing with it too much)")]
    [SerializeField] private float boostingDrag = 1.0f;
    [Tooltip("A multipler applied to steering power while in a boost, which reduces your steering capability")]
    [SerializeField] private float boostingSteerModifier = 0.4f;

    [Header("Debug")]
    [SerializeField] private bool debugSpeedometerEnable = false;
    [SerializeField] private TextMeshProUGUI debugSpeedText;
    [SerializeField] private bool debugDriftStateEnable = false;
    [SerializeField] private TextMeshProUGUI debugDriftStateText;
    [SerializeField] private bool debugDriftCompleteEnable = false;
    [SerializeField] private Image debugDriftComplete;
    [SerializeField] private bool debugBoostabilityEnable = false;
    [SerializeField] private Image debugBoostability;

    private Rigidbody sphereBody; //just reference to components of the sphere
    private Transform sphereTransform;
    private float startingDrag;

    private float leftStick, leftTrig, rightTrig; //stick ranges from -1 to 1, triggers range from 0 to 1

    private float currentForce; //the amount of force to add to the speed on any given frame
    private float rotationAmount; //the amount to turn on any given frame

    private bool reversing, stopped;

    private bool callToDrift = false; //whether the controller should attempt to drift. only used if drift is called while the left stick is neutral
    private bool drifting = false;
    private int driftDirection;
    private bool driftBoostAchieved = false;
    private float driftPoints = 0.0f;

    private bool boostInitialburst = false;
    private bool boosting = false;
    public bool Boosting { get { return boosting; } }
    [SerializeField] private bool boostAble = true;
    public bool BoostAble { set { boostAble = value; } }

    // Can be implemeneted properly after
    [Header("Alex shit")]
    public int playerIndex;
    public PhaseIndicator phaseIndicator;

    /// <summary>
    /// Standard Start. Just used to get references, get initial values, and subscribe to events
    /// </summary>
    private void Start()
    {
        // Sets horn glow to max
        phaseIndicator.SetHornGlow(phaseIndicator.hornValueMax);

        sphereBody = sphere.GetComponent<Rigidbody>();
        sphereTransform = sphere.GetComponent<Transform>();

        startingDrag = sphereBody.drag;

        inp.WestFaceEvent += DriftFlag; //subscribes to WestFaceEvent
        inp.SouthFaceEvent += BoostFlag; //subscribes to SouthFaceEvent
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

        float velocityTransformDot = Vector3.Dot(-scooterModel.transform.right, sphereBody.velocity);
        reversing = velocityTransformDot < -0.5f ? true : false;

        transform.position = sphere.transform.position - new Vector3(0, 1, 0); //makes the scooter follow the sphere
        currentForce = reversing ? (reversingPower * leftTrig) - (reversingPower * rightTrig) : (accelerationPower * rightTrig) - (brakingPower * leftTrig); //accelerating, braking, reversing

        if (drifting)
        {
            rotationAmount = Drift(); //determines the actual rotation of the larger object

            //Rotates just the model; purely for effect
            float driftTargetAmount = (driftDirection > 0) ? RangeMutations.Map_Linear(leftStick, -1, 1, 0.5f, driftTurnScalar) : RangeMutations.Map_Linear(leftStick, -1, 1, driftTurnScalar, 0.5f);
            float modelRotateAmount = 90 + driftTargetAmount * driftDirection * DRIFTING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(Mathf.Abs(currentForce), accelerationPower);
            scooterModel.localEulerAngles = Vector3.Lerp(scooterModel.localEulerAngles, new Vector3(0, modelRotateAmount, scooterModel.localEulerAngles.z), 0.2f);
        }
        else if (reversing)
        {
            rotationAmount = leftStick * reverseSteeringPower;
            rotationAmount *= -RangeMutations.Map_SpeedToSteering(Mathf.Abs(currentForce), reversingPower);

            float modelRotateAmount = 90 + (leftStick * STEERING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(Mathf.Abs(currentForce), reversingPower));
            scooterModel.localEulerAngles = Vector3.Lerp(scooterModel.localEulerAngles, new Vector3(0, modelRotateAmount, scooterModel.localEulerAngles.z), 0.2f);
        }
        else
        {
            //determines the actual rotation of the larger object
            rotationAmount = leftStick * steeringPower;
            rotationAmount *= RangeMutations.Map_SpeedToSteering(Mathf.Abs(currentForce), accelerationPower); //scales steering by speed (also prevents turning on the spot)
            rotationAmount *= boosting ? boostingSteerModifier : 1.0f; //reduces steering if boosting

            //Rotates just the model; purely for effect
            float modelRotateAmount = 90 + (leftStick * STEERING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(Mathf.Abs(currentForce), accelerationPower) * (boosting ? boostingSteerModifier : 1.0f));
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

        //Adds the boost from a successful drift
        if (driftBoostAchieved)
        {
            totalForce += driftBoost;
            driftBoostAchieved = false;
        }

        //Adds the boost from rocket boosting
        if (boostInitialburst)
        {
            totalForce += boostPower;
            boostInitialburst = false;
        }

        //Adds the force to move forward
        if (drifting)
        {
            sphereBody.AddForce(transform.forward * totalForce, ForceMode.Acceleration);
        }
        else if (reversing)
        {
            sphereBody.AddForce(scooterModel.transform.right * totalForce, ForceMode.Acceleration);
        }
        else
        {
            sphereBody.AddForce(-scooterModel.transform.right * totalForce, ForceMode.Acceleration);
        }       

        //Clamping to make it easier to come to a complete stop
        if (sphereBody.velocity.magnitude < 2 && currentForce < 2)
        {
            sphereBody.velocity = new Vector3(0, sphereBody.velocity.y, 0);
        }
    }

    private bool ReversingCheck()
    {
        if (leftTrig > 0.1f && leftTrig > rightTrig)
        {
            return true;
        }

        return false;
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
        if (!boosting && !reversing)
        {
            callToDrift = false;
            drifting = true;

            driftDirection = leftStick < 0 ? -1 : 1;
        }
        else
        {
            callToDrift = true;
        }
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
        return steeringPower * driftDirection * scaledInput * RangeMutations.Map_SpeedToSteering(Mathf.Abs(currentForce), accelerationPower); //scales steering by speed (also prevents turning on the spot)
    }

    /// <summary>
    /// Receives input as an event. Calls for a boost to be activated if possible
    /// </summary>
    /// <param name="WestFaceState">The state of the south face button, passed by the event</param>
    private void BoostFlag(bool SouthFaceState)
    {
        if (boostAble && !callToDrift && !drifting)
        {
            StartBoostActive();
        }
    }

    /// <summary>
    /// Sets boosting variables, waits, sets some of them back, then calls the cooldown
    /// </summary>
    /// <returns>IEnumerator boilerplate</returns>
    private IEnumerator BoostActive()
    {
        phaseIndicator.SetHornGlow(0);
        boosting = true;
        boostAble = false;
        boostInitialburst = true;
        sphereBody.drag = boostingDrag;

        // Where collision is disabled 
        ToggleCollision(true);

        yield return new WaitForSeconds(boostDuration);

        ToggleCollision(false);

        boosting = false;
        sphereBody.drag = startingDrag;

        StartBoostCooldown();
    }

    private void ToggleCollision(bool toggle)
    {
        switch (playerIndex)
        {
            case 1:
                Physics.IgnoreLayerCollision(9, 10, toggle);
                break;
            case 2:
                Physics.IgnoreLayerCollision(9, 11, toggle);
                break;
            case 3:
                Physics.IgnoreLayerCollision(9, 12, toggle);
                break;
            case 4:
                Physics.IgnoreLayerCollision(9, 13, toggle);
                break;
        }
    }

    /// <summary>
    /// Waits for the recharge duration then enables boosting again
    /// </summary>
    /// <returns>IEnumerator boilerplate</returns>
    private IEnumerator BoostCooldown()
    {
        StartCoroutine(phaseIndicator.beginHornGlow(boostRechargeTime));
        yield return new WaitForSeconds(boostRechargeTime);
        boostAble = true;
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

        if (debugBoostabilityEnable)
        {
            if (boostAble)
            {
                debugBoostability.color = Color.white;
            }
            else if (boosting)
            {
                debugBoostability.color = Color.yellow;
            }
            else
            {
                debugBoostability.color = Color.red;
            }
        }
    }


    private void StartBoostActive()
    {
        boostActiveCoroutine = BoostActive();
        StartCoroutine(boostActiveCoroutine);
    }
    private void StopBoostActive()
    {
        StopCoroutine(boostActiveCoroutine);
        boostActiveCoroutine = null;
    }

    private void StartBoostCooldown()
    {
        boostCooldownCoroutine = BoostCooldown();
        StartCoroutine(boostCooldownCoroutine);
    }
    private void StopBoostCooldown()
    {
        StopCoroutine(boostCooldownCoroutine);
        boostCooldownCoroutine = null;
    }
}

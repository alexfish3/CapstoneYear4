using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Version 3.0 of the vehicle controller. Drives by rolling a sphere collider around the world then simply matching the bike model to its position.
/// Drifting is slightly more complicated, and involves doing a bunch of math.
/// </summary>
public class BallDriving : MonoBehaviour
{
    private const float STEERING_MODEL_ROTATION = 15.0f; //How far the model rotates when steering normally
    private const float DRIFTING_MODEL_ROTATION = 30.0f; //How far the model rotates when drifting
    private const float MODEL_TILT_MULTIPLIER = 0.5f; //How much the model tilts compared to rotates
    private const float DRIFTING_MODEL_TILT_MULTIPLIER = 0.8f; //How much again the model tilts compared to rotates when drifting (compounded with the regular one; stops the scooter from just leaning all the way over)
    private const float MODEL_ROTATION_TIME = 0.2f; //How long it takes the model to rotate into full position
    private const float DRIFT_HOP_AMOUNT = 0.25f; //How high the little pre-drift hop is
    private const float DRIFT_HOP_TIME = 0.4f; //How fast the little hop is in seconds
    private const float GROUNDCHECK_DISTANCE = 1.3f; //How long the ray that checks for the ground is
    private const float CSV_RATIO = 0.35f; //Don't touch

    private IEnumerator boostActiveCoroutine;
    private IEnumerator boostCooldownCoroutine;

    [Header("Setup")]
    [Tooltip("Reference to the scooter model (specifically whatever empty is right above the first object with any actual mesh)")]
    [SerializeField] private Transform scooterModel;
    [Tooltip("Reference to the empty used for checking the scooter's normal to the ground")]
    [SerializeField] private Transform scooterNormal;
    [Tooltip("Reference to the movement sphere")]
    [SerializeField] private GameObject sphere; 
    public GameObject Sphere { get { return sphere; } }
    [Tooltip("An input manager class, from the correlated InputReceiver object")]
    [SerializeField] private InputManager inp;
    [Tooltip("Reference to the order manager object")]
    [SerializeField] private OrderHandler orderHandler;

    [Header("Speed Modifiers")]
    [Tooltip("An amorphous representation of how quickly the bike can accelerate")]
    [SerializeField] private float accelerationPower = 30.0f;
    [Tooltip("An amorphous representation of how hard the bike can brake")]
    [SerializeField] private float brakingPower = 30.0f;
    [Tooltip("An amorphous representation of how quickly the bike can reverse")]
    [SerializeField] private float reversingPower = 10.0f;
    [Tooltip("The amount of drag while falling. Improves the feel of the physics")]
    [SerializeField] private float fallingDrag = 1.0f;
    [Tooltip("The amount of speed that a ground boost patch gives")]
    [SerializeField] private float groundBoostAmount = 100.0f;
    [Tooltip("The multiplier applied to speed when in a ground slow patch")]
    [SerializeField] private float slowPatchMultiplier = 0.75f;
    [Tooltip("The multiplier applied to speed when holding the golden order")]
    [SerializeField] private float goldenOrderMultiplier = 0.95f;

    [Header("Steering")]
    [Tooltip("The 'turning power'. A slightly abstract concept representing how well the scooter can turn. Higher values represent a tighter turning circle")]
    [SerializeField] private float steeringPower = 15.0f;
    [Tooltip("The 'turning power' when reversing.")]
    [SerializeField] private float reverseSteeringPower = 40.0f;
    [Tooltip("The multiplier applied to turning when drifting. Always above 1 or there'll be no difference. Use caution when messing with this")]
    [SerializeField] private float driftTurnScalar = 1.8f;
    [Tooltip("The minimum multipler applied to drifting. It's hard to explain exactly what this is but you'll get a feel for it. ALWAYS KEEP IT LESS THAN DRIFTTURNSCALAR")]
    [SerializeField] private float driftTurnMinimum = 0.0f;
    [Tooltip("How many 'drift points' are needed to achieve the drift boost. This is a semi-arbitrary unit, though if the drift boost is being based entirely on time, 100 drift points equals 1 second")]
    [SerializeField] private float driftBoostThreshold = 100.0f;
    [Tooltip("How much time vs turning amount is factored into drift boost. 0 is full time, 1 is full turning amount")]
    [SerializeField] private float driftBoostMode = 0.0f;
    [Tooltip("The amount of speed granted by a first-tier successful drift")]
    [SerializeField] private float driftBoost1 = 5.0f;
    [Tooltip("The amount of speed granted by a second-tier successful drift")]
    [SerializeField] private float driftBoost2 = 10.0f;
    [Tooltip("The amount of speed granted by a second-tier successful drift")]
    [SerializeField] private float driftBoost3 = 15.0f;

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

    [Header("Phasing Information")]
    [SerializeField] PlayerCameraResizer cameraResizer;
    [Tooltip("The player index is what allows only the certain player to phase")]
    public int playerIndex;
    [Tooltip("This is the reference to the horn phase indicator")]
    public PhaseIndicator phaseIndicator;
    [Tooltip("Toggle to check phase status")]
    [SerializeField] bool checkPhaseStatus = false;
    [SerializeField] GameObject[] phaseRaycastPositions;
    [Tooltip("Whether the current map is set up for phase testing; Will uses this for things dwai")]
    [SerializeField] bool phaseSetMap = true;

    [Header("Debug")]
    [SerializeField] private bool debugSpeedometerEnable = false;
    [SerializeField] private TextMeshProUGUI debugSpeedText;
    [SerializeField] private bool debugDriftStateEnable = false;
    [SerializeField] private TextMeshProUGUI debugDriftStateText;
    [SerializeField] private bool debugDriftCompleteEnable = false;
    [SerializeField] private Image debugDriftComplete;
    [SerializeField] private bool debugBoostabilityEnable = false;
    [SerializeField] private Image debugBoostability;
    [SerializeField] private bool debugCSVEnable = false;
    [SerializeField] private TextMeshProUGUI debugCSV;

    private Rigidbody sphereBody; //just reference to components of the sphere
    private Transform sphereTransform;
    private Respawn respawn; // used to update the respawn point when grounded
    private float startingDrag;

    private float leftStick, leftTrig, rightTrig; //stick ranges from -1 to 1, triggers range from 0 to 1

    private float currentForce; //the amount of force to add to the speed on any given frame
    private float scaledVelocityMax; //a complicated variable derived from a bunch of testing and math which really just boils down to accelerationPower * 0.35
    private float currentVelocity; //just shorthand for the sphere's current velocity magnitude

    private float rotationAmount; //the amount to turn on any given frame

    private bool stopped, reversing, grounded;

    private bool callToDrift = false; //whether the controller should attempt to drift. only used if drift is called while the left stick is neutral
    private bool drifting = false;
    private int driftDirection;
    private bool driftBoostAchieved = false;
    private float driftPoints = 0.0f;
    private float driftBoost = 0.0f;
    private int driftTier = 0;

    private bool groundBoostFlag = false;
    private bool groundSlowFlag = false;

    private bool onMovingPlatform = false; //tells whether the player is on a moving platform
    private MovingPlatform currentMovingPlatform;
    private int movingPlatformIndex; //a local copy of the index that the current moving platform recognizes this scooter as

    private bool boostInitialburst = false;
    private bool boosting = false;
    public bool Boosting { get { return boosting; } }
    private bool boostAble = true;
    public bool BoostAble { set { boostAble = value; } }
    private bool phasing = false;

    private float csv;


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

        scaledVelocityMax = accelerationPower * CSV_RATIO;

        respawn = sphere.GetComponent<Respawn>(); // get respawn component
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

        //Assigns drag
        if (!grounded)
        {
            sphereBody.drag = fallingDrag;
        }
        else if (boosting)
        {
            sphereBody.drag = boostingDrag;
        }
        else
        {
            sphereBody.drag = startingDrag;
        }

        if (stopped)
        {
            reversing = (rightTrig < leftTrig);
        }
        if (reversing && rightTrig > leftTrig)
        {
            reversing = false;
        }

        transform.position = sphere.transform.position - new Vector3(0, 1, 0); //makes the scooter follow the sphere
        currentForce = reversing ? (reversingPower * leftTrig * (1 - rightTrig)) : (accelerationPower * rightTrig * (1 - leftTrig));
        currentForce = boosting ? accelerationPower : currentForce;
        currentVelocity = sphereBody.velocity.magnitude;
        if (currentVelocity != 0)
        {
            csv = currentForce / currentVelocity;
        }

        float modelRotateAmount;
        if (drifting)
        {
            //Determines the actual rotation of the larger object
            rotationAmount = Drift();

            //Determines model rotation
            float driftTargetAmount = (driftDirection > 0) ? RangeMutations.Map_Linear(leftStick, -1, 1, 0.5f, driftTurnScalar) : RangeMutations.Map_Linear(leftStick, -1, 1, driftTurnScalar, 0.5f);
            modelRotateAmount = 90 + driftTargetAmount * driftDirection * DRIFTING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(currentVelocity, scaledVelocityMax);
        }
        else if (reversing)
        {
            DirtyDriftDrop(); //only needed like 1% of the time but fixes a weird little collision behavior
            
            //Determines actual rotation
            rotationAmount = leftStick * reverseSteeringPower;
            rotationAmount *= -RangeMutations.Map_SpeedToSteering(currentVelocity, scaledVelocityMax);

            //Determines model rotation
            modelRotateAmount = 90 + (leftStick * STEERING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(currentVelocity, scaledVelocityMax));
        }
        else
        {
            //Determines the actual rotation of the larger object
            rotationAmount = leftStick * steeringPower;
            rotationAmount *= RangeMutations.Map_SpeedToSteering(currentVelocity, scaledVelocityMax + (boosting ? boostPower * CSV_RATIO : 0)); //scales steering by speed (also prevents turning on the spot)
            rotationAmount *= boosting ? boostingSteerModifier : 1.0f; //reduces steering if boosting

            //Determines model rotation
            modelRotateAmount = 90 + (leftStick * STEERING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(currentVelocity, scaledVelocityMax) * (boosting ? boostingSteerModifier : 1.0f));
        }

        //Applies model rotation
        Quaternion intendedRotation = Quaternion.Euler((modelRotateAmount - 90f) * MODEL_TILT_MULTIPLIER * (drifting ? DRIFTING_MODEL_TILT_MULTIPLIER : 1), modelRotateAmount, 0);
        Quaternion newRotation = Quaternion.Lerp(scooterModel.localRotation, intendedRotation, MODEL_ROTATION_TIME);
        scooterModel.localRotation = newRotation;

        //Rotates the control object
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
            switch (driftTier)
            {
                case 1:
                    driftBoost = driftBoost1;
                    break;
                case 2:
                    driftBoost = driftBoost2;
                    break;
                case 3:
                    driftBoost = driftBoost3;
                    break;
            }
            totalForce += driftBoost;
            driftBoostAchieved = false;
            driftTier = 0;
        }

        //Adds the boost from rocket boosting
        if (boostInitialburst)
        {
            totalForce += boostPower;
            boostInitialburst = false;
        }

        //Adds the boost from ground boosts
        if (groundBoostFlag)
        {
            totalForce += groundBoostAmount;
            groundBoostFlag = false;
        }

        //Applies slow from grass patches
        if (groundSlowFlag && !boosting)
        {
            totalForce *= slowPatchMultiplier;
            groundSlowFlag = false;
        }

        //Applies slow from holding the golden order
        if (orderHandler.HasGoldenOrder)
        {
            totalForce *= goldenOrderMultiplier;
        }

        //Adds the force to move forward
        if (grounded)
        {
            if (!boosting && !onMovingPlatform && respawn != null)
            {
                respawn.SetRespawnPoint();
            }

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
        }
        else if (boosting) //allows boosting in mid-air. bit of a weird implementation; possibly refactor in the future.
        {
            sphereBody.AddForce(-scooterModel.transform.right * totalForce, ForceMode.Acceleration);
        }

        //Clamping to make it easier to come to a complete stop
        if (sphereBody.velocity.magnitude < 2 && currentForce < 2)
        {
            sphereBody.velocity = new Vector3(0, sphereBody.velocity.y, 0);
            DirtyDriftDrop();
            stopped = true;
        }
        else
        {
            stopped = false;
        }

        // Enables raycasting for boosting while in a phase
        if (checkPhaseStatus && phaseSetMap)
        {
            int layerMask = 1 << 9;
            RaycastHit hit1, hit2;

            // First raycast
            bool hit1Success = Physics.Raycast(phaseRaycastPositions[0].transform.position, transform.TransformDirection(Vector3.down), out hit1, Mathf.Infinity, layerMask);

            // Second raycast
            bool hit2Success = Physics.Raycast(phaseRaycastPositions[1].transform.position, transform.TransformDirection(Vector3.down), out hit2, Mathf.Infinity, layerMask);

            // Check if either raycast hit
            if (hit1Success == true && hit2Success == true)
            {
                Debug.Log("Inside Building");
                Debug.DrawRay(phaseRaycastPositions[0].transform.position, transform.TransformDirection(Vector3.down) * 200, Color.red);
                Debug.DrawRay(phaseRaycastPositions[1].transform.position, transform.TransformDirection(Vector3.down) * 200, Color.red);
                
                phasing = true;
            }
            else if(hit1Success == false && hit2Success == false)
            {
                Debug.Log("Not Inside Building");
                Debug.DrawRay(phaseRaycastPositions[0].transform.position, transform.TransformDirection(Vector3.down) * 200, Color.white);
                Debug.DrawRay(phaseRaycastPositions[1].transform.position, transform.TransformDirection(Vector3.down) * 200, Color.white);

                cameraResizer.SwapCameraRendering(true);

                phasing = false;
                ToggleCollision(false);
                checkPhaseStatus = false;
            }
        }

        GroundCheck();
    }

    /// <summary>
    /// Used a short raycast to check whether there's a driveable surface beneath the scooter, as well as find its slope
    /// Flags grounded if there's a surface, and matches the scooter to its angle if so
    /// </summary>
    private void GroundCheck()
    {
        int lm = 513; //layers 0 and 9
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, GROUNDCHECK_DISTANCE, lm))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
        Debug.DrawRay(transform.position + Vector3.up, Vector3.down * GROUNDCHECK_DISTANCE, Color.red);

        if (grounded)
        {
            scooterNormal.up = Vector3.Lerp(scooterNormal.up, hit.normal, Time.fixedDeltaTime * 10.0f);
            scooterNormal.Rotate(0, transform.eulerAngles.y, 0);

            switch (hit.collider.tag)
            {
                case "Speed":
                    groundBoostFlag = true;
                    break;

                case "TouchGrass":
                    groundSlowFlag = true;
                    break;

                case "MovingPlatform":
                    if (currentMovingPlatform == null)
                    {
                        onMovingPlatform = true;
                        currentMovingPlatform = hit.collider.gameObject.GetComponent<MovingPlatform>();
                        if ((movingPlatformIndex = currentMovingPlatform.AddToScooterList(this)) == -1)
                        {
                            Debug.LogError("Invalid Platform Index");
                        }
                    }
                    break;

                default:
                    if (currentMovingPlatform != null)
                    {
                        onMovingPlatform = false;
                        currentMovingPlatform.RemoveFromScooterList(movingPlatformIndex);
                        currentMovingPlatform = null;
                    }
                    break;
            }
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
            if (driftTier > 0)
            {
                driftBoostAchieved = true;
            }

            DirtyDriftDrop();
        }
    }

    /// <summary>
    /// Sets drifting to true and assings its direction based on the left stick.
    /// </summary>
    private void AssignDriftState()
    {
        if (!boosting && !reversing && grounded)
        {
            callToDrift = false;
            drifting = true;

            driftDirection = leftStick < 0 ? -1 : 1;

            //Does a little hop does a little jump does a little skip
            scooterModel.parent.DOComplete();
            scooterModel.parent.DOPunchPosition(transform.up * DRIFT_HOP_AMOUNT, DRIFT_HOP_TIME, 5, 0);
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
            scaledInput = RangeMutations.Map_Linear(leftStick, -1, 1, driftTurnMinimum, driftTurnScalar);
        }
        else
        {
            scaledInput = RangeMutations.Map_Linear(leftStick, -1, 1, driftTurnScalar, driftTurnMinimum);
        }

        driftPoints += (2 * Time.deltaTime * (1 - driftBoostMode)) + (Time.deltaTime * scaledInput * driftBoostMode) * 100.0f;

        if (driftPoints > driftBoostThreshold) 
        {
            driftTier = 1;
        }
        if (driftPoints > (driftBoostThreshold * 2))
        {
            driftTier = 2;
        }
        if (driftPoints > (driftBoostThreshold * 3))
        {
            driftTier = 3;
        }

        return steeringPower * driftDirection * scaledInput * RangeMutations.Map_SpeedToSteering(currentVelocity, scaledVelocityMax); //scales steering by speed (also prevents turning on the spot)
    }

    /// <summary>
    /// Ends drifting; can work with or without boost.
    /// </summary>
    private void DirtyDriftDrop()
    {
        drifting = false;
        callToDrift = false;
        driftPoints = 0;
    }

    /// <summary>
    /// Receives input as an event. Calls for a boost to be activated if possible
    /// </summary>
    /// <param name="WestFaceState">The state of the south face button, passed by the event</param>
    private void BoostFlag(bool SouthFaceState)
    {
        if (boostAble && !callToDrift && !drifting && !reversing) //& by Tally Hall
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
        DirtyDriftDrop();

        // Where collision is disabled 
        ToggleCollision(true);

        if (cameraResizer != null)
        {
            cameraResizer.SwapCameraRendering(false);
        }

        yield return new WaitForSeconds(boostDuration);

        // Toggles to check phase status
        checkPhaseStatus = true;

        yield return new WaitForFixedUpdate();

        while (phasing)
        {
            yield return new WaitForSeconds(0.1f);
        }

        boosting = false;
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
            switch (driftTier)
            {
                case 1:
                    debugDriftComplete.enabled = true;
                    debugDriftComplete.color = Color.yellow;
                    break;
                case 2:
                    debugDriftComplete.enabled = true;
                    debugDriftComplete.color = Color.red;
                    break;
                case 3:
                    debugDriftComplete.enabled = true;
                    debugDriftComplete.color = Color.magenta;
                    break;
                default:
                    debugDriftComplete.enabled = false;
                    break;
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

        if (debugCSVEnable)
        {
            debugCSV.text = "CS/V: " + csv;
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

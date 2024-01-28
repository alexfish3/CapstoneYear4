using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEditor;

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
    private const float WHEELIE_AMOUNT = 45f; //Degrees that the scooter rotates when doing a wheelie

    private const float GROUNDCHECK_DISTANCE = 1.3f; //How long the ray that checks for the ground is
    private const float CSV_RATIO = 0.35f; //Don't touch

    private const float BRAKE_CHECK_TIME = 0.08f;
    private const float RESTING_ANGULAR_DRAG = 0.1f;
    private const float FULLBRAKE_ANGULAR_DRAG = 15.0f;
    private const float RESTING_DYNAMIC_FRICTION = 0.4f;
    private const float FULLBRAKE_DYNAMIC_FRICTION = 5.0f;
    private const float RESTING_STATIC_FRICTION = 0.4f;
    private const float FULLBRAKE_STATIC_FRICTION = 5.0f;

    private IEnumerator boostActiveCoroutine;
    private IEnumerator boostCooldownCoroutine;
    private IEnumerator brakeCheckCoroutine;
    private IEnumerator endBoostCoroutine;
    private IEnumerator spinOutTimeCoroutine;
    private IEnumerator slowdownImmunityCoroutine;

    public delegate void BoostDelegate(); // boost event stuff for the trail
    public BoostDelegate OnBoostStart;

    [Header("Setup")]
    [Tooltip("Reference to the scooter model (specifically whatever empty is right above the first object with any actual mesh)")]
    [SerializeField] private Transform scooterModel;
    [Tooltip("Reference to the empty used for checking the scooter's normal to the ground")]
    [SerializeField] private Transform scooterNormal;
    public Transform ScooterNormal { get { return scooterNormal; } }
    [Tooltip("Reference to the movement sphere")]
    [SerializeField] private GameObject sphere; 
    public GameObject Sphere { get { return sphere; } }
    [Tooltip("An input manager class, from the correlated InputReceiver object")]
    [SerializeField] private InputManager inp;
    [Tooltip("Reference to the order manager object")]
    [SerializeField] private OrderHandler orderHandler;
    [Tooltip("Reference to the left slipstream trail")]
    [SerializeField] private TrailRenderer leftSlipstreamTrail;
    [Tooltip("Reference to the right slipstream trail")]
    [SerializeField] private TrailRenderer rightSlipstreamTrail;
    [Tooltip("Reference to the shitty temp drift boost trail")]
    [SerializeField] private TrailRenderer driftTrail;
    [Tooltip("Reference to the particles basket")]
    [SerializeField] private Transform particleBasket;
    [Tooltip("Reference to the left-side sparks position")]
    [SerializeField] private Transform sparksPos1;
    [Tooltip("Reference to the right-side sparks position")]
    [SerializeField] private Transform sparksPos2;

    [Header("Speed Modifiers")]
    [Tooltip("An amorphous representation of how quickly the bike can accelerate")]
    [SerializeField] private float accelerationPower = 30.0f;
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

    [Header("Drifting")]
    [Tooltip("The multiplier applied to turning when drifting. Always above 1 or there'll be no difference. Use caution when messing with this")]
    [SerializeField] private float driftTurnScalar = 1.8f;
    [Tooltip("A scalar for how 'sidewaysey' the drifting is. Higher values are LESS sideways")]
    [SerializeField] private float driftSidewaysScalar = 3.0f;
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
    [Tooltip("Color for the sparks when at the first tier of drifting")]
    [SerializeField] private Color driftSparksTier1Color;
    [Tooltip("Color for the sparks when at the second tier of drifting")]
    [SerializeField] private Color driftSparksTier2Color;
    [Tooltip("Color for the sparks when at the third tier of drifting")]
    [SerializeField] private Color driftSparksTier3Color;
    [Tooltip("How long the player is immune to grass slowdowns after getting a drift boost")]
    [SerializeField] private float slowdownImmunityDuration;

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
    [Tooltip("How much force is used when clashing")]
    [SerializeField] private float clashForce = 50.0f;

    [Header("Slipstream")]
    [Tooltip("Maximum distance that two vehicles can be from each other to get slipstream")]
    [SerializeField] private float slipstreamDistance = 10.0f;
    [Tooltip("Minimum speed that BOTH vehicles need to be moving at to get slipstream")]
    [SerializeField] private float minimumSlipstreamSpeed = 10.0f;
    [Tooltip("How long in seconds slipstream needs to be maintained before getting the full boost")]
    [SerializeField] private float slipstreamTime = 1.5f;
    [Tooltip("The most amount of speed that the pre-boost slipstream grants")]
    [SerializeField] private float preBoostSlipstreamMax = 50.0f;
    [Tooltip("How much speed is granted from the slipstream boost")]
    [SerializeField] private float slipstreamBoostAmount = 300.0f;

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
    private Collider sphereCollider;
    private Respawn respawn; // used to update the respawn point when grounded
    private QAHandler qa;
    private float startingDrag;
    private PhysicMaterial pMat;

    private ParticleManipulator baseSpark, wideSpark, flare1Spark, flare2Spark, flare3Spark, longSpark;

    private float leftStick, leftTrig, rightTrig; //stick ranges from -1 to 1, triggers range from 0 to 1

    private float currentForce; //the amount of force to add to the speed on any given frame
    private float scaledVelocityMax; //a complicated variable derived from a bunch of testing and math which really just boils down to accelerationPower * 0.35
    private float currentVelocity; //just shorthand for the sphere's current velocity magnitude
    public float CurrentVelocity { get { return currentVelocity; } }

    private float rotationAmount; //the amount to turn on any given frame

    private bool reverseGear, forwardGear, grounded;
    private bool canDrive = true;
    private bool brakeChecking = false;
    private bool stopped = true;
    private float timeSpentChecking = 0.0f;

    private bool spinningOut = false;
    private bool wheelying = false;

    private bool callToDrift = false; //whether the controller should attempt to drift. only used if drift is called while the left stick is neutral
    private bool drifting = false;
    private int driftDirection; //-1 is drifting leftward, 1 is drifting rightward
    private bool driftBoostAchieved = false;
    private float driftPoints = 0.0f;
    private float driftBoost = 0.0f;
    private int driftTier = 0;

    private bool groundBoostFlag = false;
    private bool groundSlowFlag = false;
    private bool slowdownImmune = false;

    private bool onMovingPlatform = false; //tells whether the player is on a moving platform
    private MovingPlatform currentMovingPlatform;
    private int movingPlatformIndex; //a local copy of the index that the current moving platform recognizes this scooter as

    private bool boostInitialburst = false;
    private bool boosting = false;
    public bool Boosting { get { return boosting; } }
    private bool boostAble = true;
    public bool BoostAble { set { boostAble = value; } }
    [SerializeField] bool phasing = false;

    private float slipstreamPortion = 0.0f;

    private float csv;

    [SerializeField] private GameObject groundDetector;

    private SoundPool soundPool; // for driving noises

    public bool InsideBuilding = false;

    /// <summary>
    /// Standard Start. Just used to get references, get initial values, and subscribe to events
    /// </summary>
    private void Start()
    {
        // Sets horn glow to max
        phaseIndicator.SetHornGlow(phaseIndicator.hornValueMax);

        sphereBody = sphere.GetComponent<Rigidbody>();
        sphereTransform = sphere.GetComponent<Transform>();
        sphereCollider = sphere.GetComponent<Collider>();

        pMat = new PhysicMaterial();
        pMat.bounciness = 0.3f;
        pMat.staticFriction = RESTING_STATIC_FRICTION;
        pMat.dynamicFriction = RESTING_DYNAMIC_FRICTION;
        sphereCollider.material = pMat;

        startingDrag = sphereBody.drag;
        //baseFriction = selfPhysicsMaterial.dynamicFriction;
        //frictionDifference = brakingFriction - baseFriction;

        inp.WestFaceEvent += DriftFlag; //subscribes to WestFaceEvent
        inp.SouthFaceEvent += BoostFlag; //subscribes to SouthFaceEvent

        scaledVelocityMax = accelerationPower * CSV_RATIO;

        respawn = sphere.GetComponent<Respawn>(); // get respawn component
        soundPool = GetComponent<SoundPool>();
        qa = GetComponent<QAHandler>();

        baseSpark = particleBasket.GetChild(0).GetComponent<ParticleManipulator>();
        wideSpark = particleBasket.GetChild(1).GetComponent<ParticleManipulator>();
        flare1Spark = particleBasket.GetChild(2).GetComponent<ParticleManipulator>();
        flare2Spark = particleBasket.GetChild(3).GetComponent<ParticleManipulator>();
        flare3Spark = particleBasket.GetChild(4).GetComponent<ParticleManipulator>();
        longSpark = particleBasket.GetChild(5).GetComponent<ParticleManipulator>();

        orderHandler.GotHit += SpinOut;
        orderHandler.Clash += BounceOff;
    }

    /// <summary>
    /// Standard Update. Gets controls, updates variables, and manages many aspects of steering and speed
    /// </summary>
    private void Update()
    {
        transform.position = sphere.transform.position - new Vector3(0, 1, 0); //makes the scooter follow the sphere

        if (!canDrive)
            return;

        leftStick = inp.LeftStickValue;
        leftTrig = inp.LeftTriggerValue;
        rightTrig = inp.RightTriggerValue;

        if (currentVelocity > 0.5 && csv > 0 && ((leftTrig == 1 && !reverseGear) || (rightTrig == 1 && reverseGear)))
        {
            soundPool.PlayBrakeSound();
        }
        if (csv == 0)
        {
            soundPool.PlayIdleSound();
        }
        else
        {
            soundPool.PlayEngineSound();
        }


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

        //Checks for whether the scooter has been still long enough to be considered stopped
        currentVelocity = sphereBody.velocity.magnitude;
        if (currentVelocity > 0.1f)
        {
            csv = currentForce / currentVelocity;
            stopped = false;
            StopBrakeCheck();
        }
        else if (!brakeChecking && !stopped)
        {
            brakeChecking = true;
            StartBrakeCheck();
        }

        //When stopped, changes gear based on trigger input
        if (stopped)
        {
            reverseGear = false;
            forwardGear = false;

            if (rightTrig > leftTrig)
            {
                forwardGear = true;
                reverseGear = false;
                stopped = false;
            }
            else if (leftTrig > rightTrig)
            {
                reverseGear = true;
                forwardGear = false;
                stopped = false;
            }
        }

        if (reverseGear)
        {
            currentForce = Mathf.Max((reversingPower * leftTrig) - (reversingPower * rightTrig), 0);
            sphereBody.angularDrag = RangeMutations.Map_Linear(rightTrig, 0, 1, RESTING_ANGULAR_DRAG, FULLBRAKE_ANGULAR_DRAG);
            pMat.dynamicFriction = RangeMutations.Map_Linear(rightTrig, 0, 1, RESTING_DYNAMIC_FRICTION, FULLBRAKE_DYNAMIC_FRICTION);
            pMat.staticFriction = RangeMutations.Map_Linear(rightTrig, 0, 1, RESTING_STATIC_FRICTION, FULLBRAKE_STATIC_FRICTION);
        }
        if (forwardGear) 
        {
            currentForce = Mathf.Max((accelerationPower * rightTrig) - (accelerationPower * leftTrig), 0);
            sphereBody.angularDrag = RangeMutations.Map_Linear(leftTrig, 0, 1, RESTING_ANGULAR_DRAG, FULLBRAKE_ANGULAR_DRAG);
            pMat.dynamicFriction = RangeMutations.Map_Linear(leftTrig, 0, 1, RESTING_DYNAMIC_FRICTION, FULLBRAKE_DYNAMIC_FRICTION);
            pMat.staticFriction = RangeMutations.Map_Linear(leftTrig, 0, 1, RESTING_STATIC_FRICTION, FULLBRAKE_STATIC_FRICTION);
        }

        if (boosting)
            currentForce = accelerationPower;

        float modelRotateAmount;
        if (drifting)
        {
            //Determines the actual rotation of the larger object
            rotationAmount = Drift();

            //Determines model rotation
            float driftTargetAmount = (driftDirection > 0) ? RangeMutations.Map_Linear(leftStick, -1, 1, 0.5f, driftTurnScalar) : RangeMutations.Map_Linear(leftStick, -1, 1, driftTurnScalar, 0.5f);
            modelRotateAmount = 90 + driftTargetAmount * driftDirection * DRIFTING_MODEL_ROTATION * RangeMutations.Map_SpeedToSteering(currentVelocity, scaledVelocityMax);
        }
        else if (reverseGear)
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
        Quaternion newRotation = Quaternion.Lerp(scooterModel.parent.localRotation, intendedRotation, MODEL_ROTATION_TIME);
        scooterModel.parent.localRotation = newRotation;

        //Rotates the control object
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + rotationAmount, 0), Time.deltaTime);

        DebugUIUpdate();
    }

    /// <summary>
    /// Standard FixedUpdate. Handles the actual movement.
    /// </summary>
    private void FixedUpdate()
    {
        if (!canDrive)
            return;

        float totalForce = currentForce;

        //Adds the boost from a successful drift
        if (driftBoostAchieved)
        {
            soundPool.PlayMiniBoost();
            switch (driftTier)
            {
                case 1:
                    driftBoost = driftBoost1;
                    driftTrail.time = 0.5f;
                    break;
                case 2:
                    driftBoost = driftBoost2;
                    driftTrail.time = 1.0f;
                    break;
                case 3:
                    driftBoost = driftBoost3;
                    driftTrail.time = 1.5f;
                    break;
            }
            totalForce += driftBoost;
            driftBoostAchieved = false;
            driftTier = 0;
            StartSlowdownImmunity();
        }

        driftTrail.time -= Time.fixedDeltaTime;

        //Adds the boost from rocket boosting
        if (boostInitialburst)
        {
            totalForce += boostPower;
            boostInitialburst = false;
        }

        //Adds the boost from slipstream
        SetSlipstreamTrails(leftSlipstreamTrail.time - Time.fixedDeltaTime);
        totalForce += Slipstream(); //most of the time Slipstream just returns 0

        //Adds the boost from ground boosts
        if (groundBoostFlag)
        {
            totalForce += groundBoostAmount;
            groundBoostFlag = false;
        }

        //Applies slow from grass patches
        if (groundSlowFlag && !boosting )
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
            if (!onMovingPlatform && respawn != null)
            {
                respawn.LastGroundedPos = sphere.transform.position;
            }

            if (forwardGear)
            {
                if (drifting)
                {
                    sphereBody.AddForce((driftDirection == 1 ? ((driftSidewaysScalar * transform.forward) - transform.right).normalized : ((driftSidewaysScalar * transform.forward) + transform.right).normalized) * totalForce, ForceMode.Acceleration);
                }
                else if (boosting)
                {
                    sphereBody.AddForce(transform.forward * totalForce, ForceMode.Acceleration);
                }
                else
                {
                    sphereBody.AddForce(-scooterModel.transform.right * totalForce, ForceMode.Acceleration);
                }
            }
            else if (reverseGear)
            {
                sphereBody.AddForce(scooterModel.transform.right * totalForce, ForceMode.Acceleration);
            }
        }
        else if (boosting) //allows boosting in mid-air. bit of a weird implementation; possibly refactor in the future.
        {
            sphereBody.AddForce(transform.forward * totalForce, ForceMode.Acceleration);
        }

        //Clamping to make it easier to come to a complete stop
        if (sphereBody.velocity.magnitude < 3)
        {
            DirtyDriftDrop();

            if (sphereBody.velocity.magnitude < 1 && currentForce < 2)
            {
                sphereBody.velocity = new Vector3(0, sphereBody.velocity.y, 0);
            }
        }

        if (rightTrig < 0.05f)
        {
            DirtyDriftDrop();
        }

        // Enables raycasting for boosting while in a phase
        if (phaseSetMap)
        {
            int layerMask = 1 << 9;
            RaycastHit hit1, hit2;

            // First raycast
            bool hit1Success = Physics.Raycast(phaseRaycastPositions[0].transform.position, transform.TransformDirection(Vector3.down), out hit1, Mathf.Infinity, layerMask);
            // Second raycast
            bool hit2Success = Physics.Raycast(phaseRaycastPositions[1].transform.position, transform.TransformDirection(Vector3.down), out hit2, Mathf.Infinity, layerMask);

            if (hit1Success == false && hit2Success == false && checkPhaseStatus)
            {
                Debug.Log("Stop Phasing Inside Building");
                Debug.DrawRay(phaseRaycastPositions[0].transform.position, transform.TransformDirection(Vector3.down) * 200, Color.white);
                Debug.DrawRay(phaseRaycastPositions[1].transform.position, transform.TransformDirection(Vector3.down) * 200, Color.white);

                cameraResizer.SwapCameraRendering(true);

                phasing = false;
                ToggleCollision(false);
                checkPhaseStatus = false;
                InsideBuilding = false;
                phaseIndicator.SetPhaseCam(false);
                soundPool.StopPhaseSound();
            }
            // Check if either raycast hit
            else if (hit1Success == false && hit2Success == false && InsideBuilding == true)
            {
                Debug.Log("Not Inside Building");
                InsideBuilding = false;
                phaseIndicator.SetPhaseCam(false);
            }
            else if (hit1Success == true && hit2Success == true && InsideBuilding == false)
            {
                Debug.Log("Inside Building");

                phaseIndicator.SetPhaseCam(true);

                InsideBuilding = true;

                Debug.DrawRay(phaseRaycastPositions[0].transform.position, transform.TransformDirection(Vector3.down) * 200, Color.red);
                Debug.DrawRay(phaseRaycastPositions[1].transform.position, transform.TransformDirection(Vector3.down) * 200, Color.red);

                phasing = true;
                soundPool.PlayPhaseSound();
            }
        }

        GroundCheck();
    }

    /// <summary>
    /// Standard LateUpdate. Corrects model stuff when boosting
    /// </summary>
    private void LateUpdate()
    {
        if (wheelying)
        {
            scooterModel.parent.parent.localEulerAngles = new Vector3(scooterModel.parent.parent.localEulerAngles.x, 0, 0);
        }
    }

    /// <summary>
    /// Times how long the player has stood still, and marks them as stopped after a certain threshold
    /// </summary>
    /// <returns>Boilerplate IEnumerator</returns>
    private IEnumerator BrakeCheck()
    {
        timeSpentChecking = 0.0f;

        while (timeSpentChecking < BRAKE_CHECK_TIME)
        {
            timeSpentChecking += Time.deltaTime;
            yield return null;
        }

        stopped = true;
        brakeChecking = false;
    }

    /// <summary>
    /// Used a short raycast to check whether there's a driveable surface beneath the scooter, as well as find its slope
    /// Flags grounded if there's a surface, and matches the scooter to its angle if so
    /// </summary>
    private void GroundCheck()
    {
        int lm = 513; //layers 0 and 9
        RaycastHit hit;

        if (Physics.Raycast(groundDetector.transform.position, Vector3.down, out hit, GROUNDCHECK_DISTANCE))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

        Debug.DrawRay(groundDetector.transform.position, Vector3.down, Color.red, GROUNDCHECK_DISTANCE);

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
        if (!boosting && !reverseGear && grounded)
        {
            soundPool.PlayDriftSound();
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
            DriftSparkSet(1);
        }
        if (driftPoints > (driftBoostThreshold * 2))
        {
            driftTier = 2;
            DriftSparkSet(2);
        }
        if (driftPoints > (driftBoostThreshold * 3))
        {
            driftTier = 3;
            DriftSparkSet(3);
        }

        return steeringPower * driftDirection * scaledInput * RangeMutations.Map_SpeedToSteering(currentVelocity, scaledVelocityMax); //scales steering by speed (also prevents turning on the spot)
    }

    /// <summary>
    /// Ends drifting; can work with or without boost.
    /// </summary>
    private void DirtyDriftDrop()
    {
        soundPool.StopDriftSound();
        drifting = false;
        callToDrift = false;
        driftPoints = 0;
        DriftSparkSet(0);
    }

    /// <summary>
    /// Sets the sparks when drifting, aligns them to the correct angle
    /// </summary>
    /// <param name="tier">Which drift tier applies. 0 is no tier.</param>
    private void DriftSparkSet(int tier)
    {
        if (driftDirection > 0)
        {
            particleBasket.position = sparksPos2.position;
            particleBasket.localEulerAngles = new Vector3(0, 160, 0);
        }
        else
        {
            particleBasket.position = sparksPos1.position;
            particleBasket.localEulerAngles = new Vector3(0, 20, 0);
        }

        switch (tier) 
        {
            case 0:
                baseSpark.gameObject.SetActive(false);
                wideSpark.gameObject.SetActive(false);
                flare1Spark.gameObject.SetActive(false);
                longSpark.gameObject.SetActive(false);
                flare2Spark.gameObject.SetActive(false);
                flare3Spark.gameObject.SetActive(false);
                break;

            case 1:
                baseSpark.gameObject.SetActive(true);
                wideSpark.gameObject.SetActive(true);
                flare1Spark.gameObject.SetActive(true);

                baseSpark.StartColor = driftSparksTier1Color;
                wideSpark.StartColor = driftSparksTier1Color;
                flare1Spark.StartColor = driftSparksTier1Color;

                break;

            case 2:
                baseSpark.gameObject.SetActive(true);
                wideSpark.gameObject.SetActive(true);
                flare1Spark.gameObject.SetActive(true);
                longSpark.gameObject.SetActive(true);
                flare2Spark.gameObject.SetActive(true);

                baseSpark.StartColor = driftSparksTier2Color;
                wideSpark.StartColor = driftSparksTier2Color;
                flare1Spark.StartColor = driftSparksTier2Color;
                longSpark.StartColor = driftSparksTier2Color;
                flare2Spark.StartColor = driftSparksTier2Color;

                break;

            case 3:
                baseSpark.gameObject.SetActive(true);
                wideSpark.gameObject.SetActive(true);
                flare1Spark.gameObject.SetActive(true);
                longSpark.gameObject.SetActive(true);
                flare2Spark.gameObject.SetActive(true);
                flare3Spark.gameObject.SetActive(true);

                baseSpark.StartColor = driftSparksTier3Color;
                wideSpark.StartColor = driftSparksTier3Color;
                flare1Spark.StartColor = driftSparksTier3Color;
                longSpark.StartColor = driftSparksTier3Color;
                flare2Spark.StartColor = driftSparksTier3Color;
                flare3Spark.StartColor = driftSparksTier3Color;
                    
                break;
        }
    }

    /// <summary>
    /// Simple timer for how long the player is immune to grass slowdowns after getting a drift boost
    /// </summary>
    /// <returns>Boilerplate IEnumerator</returns>
    private IEnumerator SlowdownImmunity()
    {
        slowdownImmune = true;
        yield return new WaitForSeconds(slowdownImmunityDuration);
        slowdownImmune = false;
    }

    /// <summary>
    /// Receives input as an event. Calls for a boost to be activated if possible
    /// </summary>
    /// <param name="WestFaceState">The state of the south face button, passed by the event</param>
    private void BoostFlag(bool SouthFaceState)
    {
        if (boostAble && !callToDrift && !drifting && !reverseGear) //& by Tally Hall
        {
            StartBoostActive();
            OnBoostStart?.Invoke();
            soundPool.PlayBoostActivate();
            qa.Boosts++;
        }
    }

    private IEnumerator BoostActive()
    {
        boosting = true;
        boostAble = false;
        boostInitialburst = true;
        DirtyDriftDrop();

        ToggleCollision(true);

        if (cameraResizer != null)
        {
            cameraResizer.SwapCameraRendering(false);
        }

        scooterModel.parent.parent.DOComplete();

        Tween wheelie = scooterModel.parent.parent.DORotate(new Vector3(-WHEELIE_AMOUNT, 0, 0), 0.8f * 1.6f, RotateMode.LocalAxisAdd);
        wheelie.SetEase(Ease.OutQuint);
        wheelie.SetRelative(true);
        Tween wheelieEnd = scooterModel.parent.parent.DORotate(new Vector3(WHEELIE_AMOUNT, 0, 0), 0.8f * 1.6f, RotateMode.LocalAxisAdd);
        wheelieEnd.SetEase(Ease.OutBounce);
        wheelieEnd.SetRelative(true);

        Sequence mySeq = DOTween.Sequence();
        mySeq.Append(wheelie);
        mySeq.Append(wheelieEnd);

        wheelying = true;

        // Start the glow depletion coroutine and wait for it to complete
        yield return StartCoroutine(phaseIndicator.GlowDeplete(0.8f * boostDuration));

        Debug.Log("Glow depletion complete");

        // After glow depletion is complete, proceed with the rest of the boost logic
        StartEndBoost(wheelie, wheelieEnd);
    }

    private IEnumerator EndBoost(Tween wheelie = null, Tween wheelieEnd = null)
    {
        // Toggles to check phase status
        checkPhaseStatus = true;

        yield return new WaitForFixedUpdate();

        while (phasing)
        {
            yield return new WaitForSeconds(0.1f);
        }

        boosting = false;

        if (wheelie != null && wheelieEnd != null) 
        {
            wheelie.Complete();

            yield return wheelieEnd.WaitForCompletion();

            scooterModel.parent.parent.localEulerAngles = Vector3.zero;
            wheelying = false;
        }

        StartBoostCooldown();
    }

    /// <summary>
    /// Sets collision layers for boost phasing
    /// </summary>
    /// <param name="toggle">Whether to phase or not</param>
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
        yield return StartCoroutine(phaseIndicator.GlowCharge(boostRechargeTime));
        boostAble = true;
    }

    /// <summary>
    /// Increases speed slowly while behind another vehicle and facing in approximately the same direction. 
    /// After a certain amount of time tailing them like that, grants a further burst of speed
    /// </summary>
    /// <returns>How much speed should be added based on current slipstream status</returns>
    private float Slipstream()
    {
        BallDriving caddy = null;
        bool slipstreamRaysAligned = false;
        bool caddySpeedMet = false;
        bool selfSpeedMet = currentVelocity > minimumSlipstreamSpeed;

        int lm = 128; //layer 7
        RaycastHit hit;

        Debug.DrawRay(transform.position + Vector3.up, scooterNormal.forward * slipstreamDistance, Color.green);
        if (Physics.Raycast(transform.position + Vector3.up, scooterNormal.forward, out hit, slipstreamDistance, lm)) //checks forward ray
        {
            caddy = hit.collider.gameObject.GetComponent<BallDriving>();
            caddySpeedMet = caddy.CurrentVelocity > minimumSlipstreamSpeed;

            RaycastHit secondHit;

            if (Physics.Raycast(caddy.transform.position + Vector3.up, -caddy.ScooterNormal.forward, out secondHit, slipstreamDistance, lm)) //checks reciprocal ray
            {
                slipstreamRaysAligned = true;
            }
        }

        //Updates slipstream time. Decreases at twice the speed it increases
        if (slipstreamRaysAligned && caddySpeedMet && selfSpeedMet)
        {
            slipstreamPortion += Time.fixedDeltaTime;
        }
        else
        {
            slipstreamPortion -= (Time.fixedDeltaTime * 2.0f);
        }
        slipstreamPortion = Mathf.Clamp(slipstreamPortion, 0.0f, slipstreamTime);

        float slipStreamScalar = RangeMutations.Map_Linear(slipstreamPortion, 0.0f, slipstreamTime, 0.0f, 1.0f);

        //Returns a certain amount of speed based on the current amount of slipstream
        if (slipstreamPortion == slipstreamTime)
        {
            slipstreamPortion = 0.0f;
            if (caddy != null) { caddy.SetSlipstreamTrails(0.0f); }
            return slipstreamBoostAmount;
        }
        else
        {
            if (caddy != null) { caddy.SetSlipstreamTrails(slipStreamScalar - 0.2f); }
            return slipStreamScalar * preBoostSlipstreamMax;
        }
    }

    /// <summary>
    /// Sets the length of the slipstream indication trails. Called by a *different* player
    /// </summary>
    /// <param name="trailAmount">How long the trail should go</param>
    public void SetSlipstreamTrails(float trailAmount)
    {
        leftSlipstreamTrail.time = trailAmount;
        rightSlipstreamTrail.time = trailAmount;
    }

    /// <summary>
    /// Starts a spinout when called. Meant to be invoked by events from OrderHandler
    /// </summary>
    private void SpinOut()
    {
        if (!spinningOut)
        {
            canDrive = false;
            spinningOut = true;
            DirtyDriftDrop();
            StartSpinOutTime();
        }
    }

    /// <summary>
    /// Spins the model. Uses the outBack easing function to overshoot slightly, then correct.
    /// Simultaneously rocks the model side to side. Once done, returns the ability to drive.
    /// </summary>
    /// <returns>IEnumerator boilerplate</returns>
    private IEnumerator SpinOutTime()
    {
        scooterModel.parent.DOComplete(); //make sure nothing's in the wrong place
        float tweenTime = 1.0f;

        Tween spinning = scooterModel.parent.DORotate(new Vector3(scooterModel.parent.rotation.x, 360, scooterModel.parent.rotation.z), tweenTime, RotateMode.LocalAxisAdd);
        spinning.SetEase(Ease.OutBack); //an easing function which dictates a steep climb, slight overshoot, then gradual correction

        Tween rocking = scooterModel.DOShakeRotation(tweenTime, new Vector3(10, 0, 0), 10, 90, true, ShakeRandomnessMode.Harmonic); //rocks the scooter around its long axis

        yield return spinning.WaitForCompletion();

        canDrive = true;
        spinningOut = false;
        scooterModel.parent.localEulerAngles = new Vector3(scooterModel.rotation.x, 90, scooterModel.rotation.z); //prevents the model from misaligning
    }

    /// <summary>
    /// Slams balls together
    /// </summary>
    /// <param name="opponent">The other person in the collision</param>
    private void BounceOff(OrderHandler opponent)
    {
        if (phasing)
            return;

        Debug.Log("BOUNCE " + this.gameObject.transform.parent.name);

        Rigidbody opponentBall = opponent.gameObject.GetComponent<BallDriving>().Sphere.GetComponent<Rigidbody>(); //woof

        Vector3 difference = (sphereBody.position - opponentBall.position).normalized;
        difference.y = 0.15f;
        difference.Normalize();

        sphereBody.AddForce(difference * clashForce, ForceMode.Impulse);
        //StartEndBoost();
    }

    public void FreezeBall(bool toFreeze)
    {
        canDrive = toFreeze;

        sphereBody.constraints = toFreeze ? RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ : RigidbodyConstraints.None;
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
        if (boostActiveCoroutine != null)
        {
            StopCoroutine(boostActiveCoroutine);
            boostActiveCoroutine = null;
        }
    }

    private void StartBoostCooldown()
    {
        boostCooldownCoroutine = BoostCooldown();
        StartCoroutine(boostCooldownCoroutine);
    }
    private void StopBoostCooldown()
    {
        if (boostCooldownCoroutine != null)
        {
            StopCoroutine(boostCooldownCoroutine);
            boostCooldownCoroutine = null;
        }
    }

    private void StartEndBoost(Tween wheelie = null, Tween wheelieEnd = null)
    {
        Tween w = wheelie == null ? null : wheelie;
        Tween wE = wheelieEnd == null ? null : wheelieEnd;

        endBoostCoroutine = EndBoost(w, wE);
        StartCoroutine(endBoostCoroutine);
    }

    private void StopEndBoost()
    {
        if (endBoostCoroutine != null)
        {
            StopCoroutine(endBoostCoroutine);
            endBoostCoroutine = null;
        }
    }

    private void StartSpinOutTime()
    {
        spinOutTimeCoroutine = SpinOutTime();
        StartCoroutine(spinOutTimeCoroutine);
    }
    private void StopSpinOutTime()
    {
        if (spinOutTimeCoroutine != null)
        {
            StopCoroutine(spinOutTimeCoroutine);
            spinOutTimeCoroutine = null;
        }
    }

    private void StartBrakeCheck()
    {
        brakeCheckCoroutine = BrakeCheck();
        StartCoroutine(brakeCheckCoroutine);
    }
    private void StopBrakeCheck()
    {
        if (brakeCheckCoroutine != null)
        {
            StopCoroutine(brakeCheckCoroutine);
            brakeCheckCoroutine = null;
        }
        brakeChecking = false;
    }

    private void StartSlowdownImmunity()
    {
        StopSlowdownImmunity();
        slowdownImmunityCoroutine = SlowdownImmunity();
        StartCoroutine(slowdownImmunityCoroutine);
    }
    private void StopSlowdownImmunity()
    {
        if (slowdownImmunityCoroutine != null)
        {
            StopCoroutine(slowdownImmunityCoroutine);
            slowdownImmunityCoroutine = null;
        }
    }
}

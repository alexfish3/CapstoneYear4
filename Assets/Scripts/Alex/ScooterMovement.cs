using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class ScooterMovement : MonoBehaviour
{
    private IEnumerator boostCoroutine;

    [SerializeField] PlayerInput playerInput;
    [SerializeField] Floorchecker floorchecker;
    public Vector2 playerMovement;

    [SerializeField] float currentSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float acceleration;

    [Header("Camera Values")]
    public GameObject cameraHolder;
    public float rotationControl;

    [Header("Steer Values")]
    float steerControl;
    public float turningValue;
    public GameObject wheel;

    [Header("Accelerating Info")]
    public bool accelerating;

    [Header("Braking Info")]
    public bool braking;
    public float brakingSpeed;

    [Header("Boost Info")]
    public int boostCount;
    public float boostSpeed;
    public float boostDuration;
    public bool boosting;
    bool canBoost = true;
    private bool boostCall;

    [Header("Stealing Info")]
    

    // package info
    private Package heldPackage;
    [HideInInspector] public float score = 0;

    Rigidbody rb;
    AudioSource aud;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
        //GameManager.Instance.AddPlayer(this);
    }

    private void FixedUpdate()
    {
        //Vector3 cameraRotation = new Vector3(cameraHolder.transform.rotation.x, cameraHolder.transform.rotation.y + rotationControl, cameraHolder.transform.rotation.z);
        //cameraHolder.transform.rotation = Quaternion.Euler(cameraRotation);

        if (!floorchecker.Grounded)
            return;

        PlayerMove();
    }

    private void PlayerMove()
    {
        rb.rotation *= Quaternion.AngleAxis((steerControl * turningValue), Vector3.up);

        // Updates current speed based on acceleration, caps at max speed
        if (accelerating)
        {
            aud.Play();
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += acceleration * Time.deltaTime;
            }
        }
        else
        {
            aud.Stop();
            float targSpeed = currentSpeed - (acceleration * Time.deltaTime);
            currentSpeed = Mathf.Clamp(targSpeed, 0, 2048);
        }

        if (braking)
        {
            if (currentSpeed > 0)
            {
                float targSpeed = currentSpeed - (brakingSpeed * Time.deltaTime);
                currentSpeed = Mathf.Clamp(targSpeed, 0, 2048);
            }
        }

        rb.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);

        if (boostCall && boostCount > 0 && canBoost)
        {
            StartBoost();
        }
    }


    private void StartBoost()
    {
        boostCoroutine = Boost();
        StartCoroutine(boostCoroutine);
    }

    private void StopBoost()
    {
        StopCoroutine(boostCoroutine);
        boostCoroutine = null;
    }

    private IEnumerator Boost()
    {
        canBoost = false; 
        boostCount--;
        boosting = true;

        rb.AddForce(transform.forward * boostSpeed, ForceMode.Impulse);

        yield return new WaitForSeconds(boostDuration);

        boosting = false;
        canBoost = true;
    }


    public void OnMoveController(CallbackContext context)
    {
        playerMovement = context.ReadValue<Vector2>();
    }

    public void OnSteer(CallbackContext context)
    {
        steerControl = context.ReadValue<float>();
    }
    public void OnCameraRotation(CallbackContext context)
    {
        rotationControl = context.ReadValue<float>();
    }

    public void OnAccelerate(CallbackContext context)
    {
        if (context.performed)
        {
            accelerating = true;
        }
        else if (context.canceled)
        {
            accelerating = false;
        }
    }

    public void OnBrake(CallbackContext context)
    {
        if (context.performed)
        {
            braking = true;
        }
        else if (context.canceled)
        {
            braking = false;
        }
    }

    public void OnBoost(CallbackContext context)
    {
        if (context.performed)
        {
            boostCall = true;

        }
        else if (context.canceled)
        {
            boostCall = false;
        }
    }

    public void PickupPackage(Package package)
    {
        if (heldPackage == null)
        {
            heldPackage = package;
        }
    }
    public void DropPackage(Package package, bool earnPoints)
    {
        if(earnPoints)
        {
            score += package.points;
        }
        heldPackage = null;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            ScooterMovement player = other.GetComponent<ScooterMovement>();
            if(player.boosting && heldPackage != null)
            {
                heldPackage.Stolen(player);
            }
        }
    }
}

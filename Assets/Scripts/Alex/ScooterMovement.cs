using System.Collections;
using System.Collections.Generic;
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

    [Header("Boost Info")]
    public int boostCount;
    public float boostSpeed;
    public float boostDuration;
    public bool boosting;
    bool canBoost = true;
    private bool boostCall;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += acceleration * Time.deltaTime;
            }
        }
        else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= acceleration * 5 * Time.deltaTime;
            }
            else if(currentSpeed <= 0)
            {
                currentSpeed = 0;
            }
        }

        rb.AddForce(transform.forward * currentSpeed * 50, ForceMode.Acceleration);

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

}

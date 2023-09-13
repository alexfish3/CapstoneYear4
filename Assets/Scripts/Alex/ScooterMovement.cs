using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class ScooterMovement : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
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
    public bool boosting;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //Vector3 cameraRotation = new Vector3(cameraHolder.transform.rotation.x, cameraHolder.transform.rotation.y + rotationControl, cameraHolder.transform.rotation.z);
        //cameraHolder.transform.rotation = Quaternion.Euler(cameraRotation);

        PlayerMove();
    }

    private void PlayerMove()
    {
        gameObject.transform.rotation *= Quaternion.AngleAxis((steerControl * turningValue), Vector3.up);

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
            currentSpeed = 0;
        }

        //// Moving Forward
        //if (accelerating && rb.velocity.magnitude <= maxSpeed)
        //{
        //    ////Vector3 playerTransform = new Vector3(playerMovement.x, 0, playerMovement.y);
        //    //rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
        //}

        if(boosting == false)
        {
            rb.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);
            //rb.velocity = new Vector3(0, 0, currentSpeed);
        }
        else
        {
            rb.velocity = new Vector3(0, 0, (currentSpeed * 2));
        }

        //// Boost Forward
        //if (accelerating && boostCount > 0 && boosting)
        //{
        //    rb.AddForce(transform.forward * 2, ForceMode.Impulse);
        //}

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
            boosting = true;
        }
        else if (context.canceled)
        {
            boosting = false;
        }
    }

}

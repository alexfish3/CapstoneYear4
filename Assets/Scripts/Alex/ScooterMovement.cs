using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class ScooterMovement : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    public Vector2 playerMovement;

    [SerializeField] float accelerationValue;
    [SerializeField] float speed;
    [SerializeField] float maxSpeed;

    [Header("Steer Values")]
    public float steerControl;

    [Header("Accelerating Info")]
    public bool accelerating;

    [Header("Braking Info")]
    public bool braking;


    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        PlayerMove();
    }

    private void PlayerMove()
    {

        if (accelerating && rb.velocity.magnitude <= maxSpeed)
        {
            Vector3 playerTransform = new Vector3(playerMovement.x, 0, playerMovement.y);
            rb.AddForce(playerTransform * speed, ForceMode.Acceleration);
        }
    }

    public void OnMoveController(CallbackContext context)
    {
        playerMovement = context.ReadValue<Vector2>();
    }

    public void OnSteer(CallbackContext context)
    {
        steerControl = context.ReadValue<float>();
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


}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// This class will respawn the player if they fall into the water.
/// </summary>
public class Respawn : MonoBehaviour
{
    //Variables
    private Vector3 respawnPoint;
    private Quaternion initialRotation;
    private Quaternion endRotation;
    private Quaternion controlRotation;

    [Tooltip("The height to lift the player above the map.")]
    [SerializeField] private float liftHeight = 5.0f; // The height to lift the player above the map

    [Tooltip("The height to start the lift at. (Should be below the ground)")]
    [SerializeField] private float startingLiftHeight = -2.0f; // The height to start the lift at

    [Tooltip("The time it takes to rotate 180 degrees.")]
    [SerializeField] private float respawnDuration = 2.0f; // The time it takes to rotate 180 degrees

    [Tooltip("The time it takes to lift the player between startingLiftHeight to liftHeight.")]
    [SerializeField] private float liftDuration = 2.0f; // The time it takes to lift the player above the ground

    [Tooltip("The prefab for the gravestone model.)")]
    [SerializeField] private GameObject respawnGravestone; // Gravestone model to spawn in during respawn

    private bool isRotating = false;

    [Tooltip("Reference to the control game object.")]
    [SerializeField] private GameObject control;
    private OrderHandler orderHandler;

    private void Start()
    {
        orderHandler = control.GetComponent<OrderHandler>();
    }

    /// <summary>
    /// This method sets the respawn point.
    /// </summary>
    public void SetRespawnPoint(bool reversing)
    {
        if (reversing)
        {
            respawnPoint = gameObject.transform.position + control.transform.forward * 5;
            initialRotation = transform.rotation * Quaternion.Euler(1,180,0);
            controlRotation = control.transform.rotation * Quaternion.Euler(1,180,0);
        }
        else
        {
            respawnPoint = gameObject.transform.position + control.transform.forward * -5;
            initialRotation = transform.rotation;
            controlRotation = control.transform.rotation;
        }
    }

    /// <summary>
    /// This coroutine lerps the player's position from their current position to the respawn point position + liftHeight.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawningPlayer()
    {
        isRotating = true;
        endRotation = initialRotation * Quaternion.Euler(1, 180, 0); // Rotate 180 degrees from initial rotation
        Quaternion endControlRotation = controlRotation * Quaternion.Euler(1, 180, 0);
        float elapsedTime = 0;
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = respawnPoint + Vector3.up * startingLiftHeight; // Change height to position before lifting
        // Moving player to respawn position below ground
        while (elapsedTime < respawnDuration)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, endRotation, elapsedTime / respawnDuration);
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / respawnDuration);
            control.transform.rotation = Quaternion.Slerp(controlRotation, endControlRotation, elapsedTime / respawnDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        transform.position = targetPosition;
        control.transform.rotation = endControlRotation;

        isRotating = false;


        // Resetting variables
        elapsedTime = 0;
        initialPosition = transform.position;
        targetPosition = initialPosition + Vector3.up * liftHeight;

        // Lifting player above ground
        while (elapsedTime < liftDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / liftDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Turn gravity and collider back on
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<SphereCollider>().enabled = true;


    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collider: " + other.tag);
        if(other.tag == "Water")
        {
            // Turning these off fixes camera jittering on respawn
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<SphereCollider>().enabled = false;
            orderHandler.DropEverything(respawnPoint);

            Instantiate(respawnGravestone, (respawnPoint), controlRotation * Quaternion.Euler(1,180,0)); // spawn respawn gravestone
            StartCoroutine(RespawningPlayer());
        }
    }
}

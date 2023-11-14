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
    private Quaternion controlRotation;

    [Tooltip("The height to lift the player above the map.")]
    [SerializeField] private float liftHeight = 5.0f; // The height to lift the player above the map

    [Tooltip("The height to start the lift at. (Should be below the ground)")]
    [SerializeField] private float startingLiftHeight = -2.0f; // The height to start the lift at

    [Tooltip("The time it takes to rotate 180 degrees.")]
    [SerializeField] private float respawnDuration = 2.0f; // The time it takes to rotate 180 degrees

    [Tooltip("The time it takes to lift the player between startingLiftHeight to liftHeight.")]
    [SerializeField] private float liftDuration = 5.0f; // The time it takes to lift the player above the ground

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
    public void SetRespawnPoint()
    {
        respawnPoint = gameObject.transform.position;
    }

    /// <summary>
    /// This coroutine lerps the player's position from their current position to the respawn point position + liftHeight.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawningPlayer()
    {
        isRotating = true;
        Vector3 rotationRespawn = new Vector3(respawnPoint.x, transform.position.y, respawnPoint.z);
        Vector3 rotationControl = new Vector3(respawnPoint.x, control.transform.position.y, respawnPoint.z);
        float elapsedTime = 0;
        Vector3 initialPosition = transform.position;
        transform.rotation = Quaternion.LookRotation((rotationRespawn - transform.position)) * Quaternion.Euler(0, 180, 0);

        respawnPoint += transform.forward * -10;
        Vector3 targetPosition = respawnPoint;// + Vector3.up * startingLiftHeight; // Change height to position before lifting
        control.transform.rotation = Quaternion.LookRotation((rotationControl - control.transform.position)) * Quaternion.Euler(0, 180, 0);

        // Moving player to respawn position below ground
        while (elapsedTime < respawnDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / respawnDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        control.transform.rotation *= Quaternion.Euler(0,180,0);
        Instantiate(respawnGravestone, (respawnPoint), control.transform.rotation); // spawn respawn gravestone

        transform.position = targetPosition;
        isRotating = false;
        control.GetComponent<BallDriving>().enabled = true;


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
            GetComponent<Rigidbody>().velocity = Vector3.zero; // set velocity to 0 on respawn
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<SphereCollider>().enabled = false;
            control.GetComponent<BallDriving>().enabled = false; // disable player control on respawn
            orderHandler.DropEverything(respawnPoint);

            StartCoroutine(RespawningPlayer());
        }
    }
}

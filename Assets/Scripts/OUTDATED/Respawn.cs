using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameObject respawnGravestone;

    private bool isRotating = false;

    [Tooltip("Reference to the control game object.")]
    [SerializeField] private GameObject control;

    /// <summary>
    /// This method sets the respawn point. It's called when the player exits the respawn collider on the map. AKA falls off the edge.
    /// </summary>
    public void SetRespawnPoint()
    {
        respawnPoint = gameObject.transform.position;
        initialRotation = transform.rotation;
        controlRotation = control.transform.rotation;
    }

    /// <summary>
    /// This coroutine lerps the player's position from their current position to the respawn point position + liftHeight.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawningPlayer()
    {
        isRotating = true;
        endRotation = initialRotation * Quaternion.Euler(0, 180, 0); // Rotate 180 degrees from initial rotation
        Quaternion endControlRotation = controlRotation * Quaternion.Euler(0, 180, 0);
        float elapsedTime = 0;
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = respawnPoint + Vector3.up * startingLiftHeight; // Change height to position before lifting


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

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collider: " + other.tag);
        if(other.tag == "Water")
        {
            Instantiate(respawnGravestone, respawnPoint - new Vector3(0, 1, 0), Quaternion.Euler(0, transform.eulerAngles.y, 0));
            StartCoroutine(RespawningPlayer());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("leaving respawn Collider");
        if(other.tag == "RespawnCollider")
        {
            SetRespawnPoint();
        }
    }
}

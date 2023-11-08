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

    public float liftHeight = 5.0f; // The height to lift the player above the map

    public float respawnDuration = 2.0f; // The time it takes to rotate 180 degrees

    private bool isRotating = false;

    /// <summary>
    /// Starts the respawn coroutine. It's called from the BallCollision script and passes a reference to that GO to move.
    /// </summary>
    /// <param name="ballObj"></param>
    public void RespawnPlayer(GameObject ballObj)
    {
        StartCoroutine(RespawningPlayer(ballObj));
    }

    /// <summary>
    /// This method sets the respawn point. It's called when the player exits the respawn collider on the map. AKA falls off the edge.
    /// </summary>
    public void SetRespawnPoint()
    {
        respawnPoint = gameObject.transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// This coroutine lerps the player's position from their current position to the respawn point position + liftHeight.
    /// </summary>
    /// <param name="ballObj"></param>
    /// <returns></returns>
    private IEnumerator RespawningPlayer(GameObject ballObj)
    {
        isRotating = true;
        Quaternion endRotation = initialRotation * Quaternion.Euler(0, 180, 0); // Rotate 180 degrees from initial rotation

        float elapsedTime = 0;
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = respawnPoint + Vector3.up * liftHeight; // Lift the player


        while (elapsedTime < respawnDuration)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, endRotation, elapsedTime / respawnDuration);
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / respawnDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        transform.position = targetPosition;
        ballObj.transform.position = targetPosition;
        ballObj.transform.rotation = endRotation;
        isRotating = false;
    }
}

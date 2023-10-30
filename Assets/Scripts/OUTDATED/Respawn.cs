using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Respawn : MonoBehaviour
{
    //Variables
    private Vector3 respawnPoint;
    private Quaternion initialRotation;

    public float liftHeight = 5.0f; // The height to lift the player above the map

    public float respawnDuration = 2.0f; // The time it takes to rotate 180 degrees

    private bool isRotating = false;

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            StartCoroutine(RespawningPlayer());
            //RespawnPlayer();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //save player location before they drive off land
        if (other.tag == "RespawnCollider")
        {
            respawnPoint = gameObject.transform.position;
            initialRotation = transform.rotation;
        }
    }


    public void RespawnPlayer()
    {
        Debug.Log("RESAWPN PLAYER");

        //Debug.Log(respawnPoint + " respawn");

        //teleport player back onto land
        //gameObject.transform.position = respawnPoint;

        

    }

    private IEnumerator RespawningPlayer()
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
        isRotating = false;
    }
}

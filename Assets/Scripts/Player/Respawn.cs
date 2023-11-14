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

    [Tooltip("The time it takes to rotate 180 degrees.")]
    [SerializeField] private float respawnDuration = 2.0f; // The time it takes to rotate 180 degrees

    [Tooltip("The time it takes to lift the player between startingLiftHeight to liftHeight.")]
    [SerializeField] private float liftDuration = 5.0f; // The time it takes to lift the player above the ground

    [Tooltip("How far back from the ledge the player respawns | MUST BE NEGATIVE")]
    [SerializeField] private float ledgeOffset = -5f;
    private float newOffset;

    [Tooltip("The prefab for the gravestone model.)")]
    [SerializeField] private GameObject respawnGravestone; // Gravestone model to spawn in during respawn

    [Tooltip("Reference to the control game object.")]
    [SerializeField] private GameObject control;
    private OrderHandler orderHandler;
    private BallDriving ballDriving;

    private void Start()
    {
        orderHandler = control.GetComponent<OrderHandler>();
        ballDriving = control.GetComponent<BallDriving>();
        if(Mathf.Sign(ledgeOffset) != -1)
        {
            ledgeOffset = -ledgeOffset;
        }
    }

    /// <summary>
    /// This method sets the respawn point.
    /// </summary>
    public void SetRespawnPoint()
    {
        respawnPoint = gameObject.transform.position;
    }

    private void Update()
    {
        Debug.DrawRay(respawnPoint + transform.forward * newOffset, Vector3.down * 200, Color.blue);
    }
    /// <summary>
    /// This method checks if the respawn point is valid by modifying the newOffset value.
    /// </summary>
    private void CheckRespawnPoint()
    {
        int lm = (1 << 9 | 1 << 2);
        RaycastHit hit;
        newOffset = ledgeOffset;
        for(int i=0;i>ledgeOffset*2;i--)
        {
            
            if(Physics.Raycast(respawnPoint + transform.forward * newOffset, Vector3.down, out hit, Mathf.Infinity, ~lm))
            {
                Debug.Log($"offset hit: {hit.collider.name}");
                float newNewOffset = newOffset - 1; // check a position in front of the player to make sure they don't spawn in front of water
                RaycastHit newHit;
                if(Physics.Raycast(respawnPoint + transform.forward * newNewOffset, Vector3.down, out newHit, Mathf.Infinity, ~lm))
                {
                    Debug.Log($"new offset hit: {newHit.collider.name}");
                    Debug.Log($"New offset: {newOffset}");
                    return;
                }
            }
            else
            {
                newOffset--;
            }
        }
        Debug.Log("Offset is 0");
        newOffset = 0; // if all else fails, just spawn player at their last ground pos
    }

    /// <summary>
    /// This coroutine lerps the player's position from their current position to the respawn point position + liftHeight.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawningPlayer()
    {
        Vector3 rotationRespawn = new Vector3(respawnPoint.x, transform.position.y, respawnPoint.z);
        Vector3 rotationControl = new Vector3(respawnPoint.x, control.transform.position.y, respawnPoint.z);
        float elapsedTime = 0;
        Vector3 initialPosition = transform.position;
        transform.rotation = Quaternion.LookRotation((rotationRespawn - transform.position)) * Quaternion.Euler(0, 180, 0);
        CheckRespawnPoint();
        respawnPoint += transform.forward * newOffset;
        Vector3 targetPosition = new Vector3(respawnPoint.x,transform.position.y,respawnPoint.z);// + Vector3.up * startingLiftHeight; // Change height to position before lifting
        control.transform.rotation = Quaternion.LookRotation((rotationControl - control.transform.position)) * Quaternion.Euler(0, 180, 0);
        Quaternion initialRotation = control.transform.rotation;
        Quaternion targetRotation = control.transform.rotation * Quaternion.Euler(0,180,0);
        Instantiate(respawnGravestone, (respawnPoint), targetRotation); // spawn respawn gravestone

        // Moving player to respawn position below ground
        while (elapsedTime < respawnDuration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / respawnDuration);
            control.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / respawnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        control.transform.rotation = targetRotation;

        transform.position = targetPosition;
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
            //control.GetComponent<BallDriving>().enabled = false; // disable player control on respawn
            orderHandler.DropEverything(respawnPoint);

            StartCoroutine(RespawningPlayer());
        }
    }


}

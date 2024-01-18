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
    IEnumerator respawnCoroutine;
    IEnumerator respawnSetCooldown;
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

    [Tooltip("Distance between tombstone spawn and player spawn | MUST BE POSITIVE")]
    [SerializeField] private int tombstoneOffset = 2;
    private float newOffset;

    [Tooltip("The prefab for the gravestone model.)")]
    [SerializeField] private GameObject respawnGravestone; // Gravestone model to spawn in during respawn

    [Tooltip("Reference to the control game object.")]
    [SerializeField] private GameObject control;
    private OrderHandler orderHandler;
    private BallDriving ballDriving;

    [Tooltip("Layermasks for respawn logic. Should be set to building phase checker, water (ignore raycast), and ground")]
    [SerializeField] LayerMask water, ground, buildingCheck;

    // new respawn with array
    [Tooltip("How many respawn points the script stores at any given moment")]
    [SerializeField] private int sizeOfRespawnArray = 10;
    private Vector3[] respawnPoints;
    private bool shouldSetRespawn = true;
    [Tooltip("Cooldown time for setting respawn position")]
    [SerializeField] private float respawnSetInterval = 1f;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapFinalPackage += StopRespawnCoroutine;
        GameManager.Instance.OnSwapFinalPackage += ClearRespawnArray;
        GameManager.Instance.OnSwapBegin += ClearRespawnArray;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapFinalPackage -= StopRespawnCoroutine;
        GameManager.Instance.OnSwapFinalPackage -= ClearRespawnArray;
        GameManager.Instance.OnSwapBegin -= ClearRespawnArray;

    }

    private void Start()
    {
        respawnPoints = new Vector3[sizeOfRespawnArray];
        orderHandler = control.GetComponent<OrderHandler>();
        ballDriving = control.GetComponent<BallDriving>();
        if (Mathf.Sign(ledgeOffset) != -1)
        {
            ledgeOffset = -ledgeOffset;
        }
    }

    /// <summary>
    /// This method sets the respawn point.
    /// </summary>
    public void SetRespawnPoint()
    {
        if (!shouldSetRespawn) { return; }
        for (int i = 0; i < sizeOfRespawnArray - 1; i++)
        {
            respawnPoints[i] = respawnPoints[i + 1];
        }
        respawnPoints[sizeOfRespawnArray - 1] = gameObject.transform.position;
        StartRespawnCooldown();
        //respawnPoint = gameObject.transform.position;
    }

    // below is the old respawn system. will eventually be deleted but I'm too scared right now

/*    /// <summary>
    /// This method checks if the respawn point is valid by modifying the newOffset value.
    /// </summary>
    private void CheckRespawnPoint()
    {
        RaycastHit hit;
        newOffset = ledgeOffset;
        for (int i = 0; i > ledgeOffset * 2; i--)
        {
            // checks if the player is hitting the ground and water (water collider is under the ground) and not the building
            if (Physics.Raycast(respawnPoint + transform.forward * newOffset, Vector3.down, out hit, Mathf.Infinity, ground) &&
            Physics.Raycast(respawnPoint + transform.forward * newOffset, Vector3.down, out hit, Mathf.Infinity, water)
            && !Physics.Raycast(respawnPoint + transform.forward * newOffset, Vector3.down, out hit, Mathf.Infinity, buildingCheck))
            {
                float newNewOffset = newOffset - 2f; // check a position in front of the player to make sure they don't spawn in front of water
                RaycastHit newHit;

                // same thing for position slightly infront of player (so that they don't spawn on an edge or something)
                if (Physics.Raycast(respawnPoint + transform.forward * newNewOffset, Vector3.down, out newHit, Mathf.Infinity, ground) &&
                Physics.Raycast(respawnPoint + transform.forward * newNewOffset, Vector3.down, out newHit, Mathf.Infinity, water)
                && !Physics.Raycast(respawnPoint + transform.forward * newNewOffset, Vector3.down, out newHit, Mathf.Infinity, buildingCheck))
                {
                    RaycastHit tombstoneBlues; // i am going insane
                    Vector3 potentialRespawn = respawnPoint + transform.forward * newOffset;
                    // NOW we check if the tombstone will spawn weirdly. If it doesn't we can finally return.
                    if (Physics.Raycast(potentialRespawn + transform.forward * (tombstoneOffset + 1), Vector3.down, out tombstoneBlues, Mathf.Infinity, ground) &&
                    Physics.Raycast(potentialRespawn + transform.forward * (tombstoneOffset + 1), Vector3.down, out tombstoneBlues, Mathf.Infinity, water)
                    && !Physics.Raycast(potentialRespawn + transform.forward * (tombstoneOffset + 1), Vector3.down, out tombstoneBlues, Mathf.Infinity, buildingCheck))
                    {
                        return;
                    }
                    else
                    {
                        newOffset--;
                    }
                }
                else
                {
                    newOffset--;
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
        orderHandler.DropEverything(respawnPoint);
        Vector3 targetPosition = new Vector3(respawnPoint.x, respawnPoint.y - liftHeight, respawnPoint.z); // Change height to position before lifting
        control.transform.rotation = Quaternion.LookRotation((rotationControl - control.transform.position)) * Quaternion.Euler(0, 180, 0);
        Quaternion initialRotation = control.transform.rotation;
        Quaternion targetRotation = control.transform.rotation * Quaternion.Euler(0, 180, 0);
        Instantiate(respawnGravestone, respawnPoint + transform.forward * tombstoneOffset, targetRotation); // spawn respawn gravestone

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
        StopRespawnCoroutine();
    }*/

    /// <summary>
    /// This method checks the array of respawn points and returns one that can be used.
    /// </summary>
    /// <returns>Coordinates of respawn point that can be used, if none are optimal return the last known one.</returns>
    private Vector3 GetRespawnPoint()
    {
        RaycastHit hit;
        bool groundCast, waterCast, buildingCast;
        for (int i = sizeOfRespawnArray - 2; i >= 0; i--)
        {
            bool validSpawn = true;
            Vector3 t = respawnPoints[i];
            for(int j=-1;j<=1;j+=1) // checks one behind, one at, and one ahead to validate respawn point
            {
                groundCast = Physics.Raycast(t + transform.forward * j, Vector3.down, out hit, Mathf.Infinity, ground);
                waterCast = Physics.Raycast(t + transform.forward * j, Vector3.down, out hit, Mathf.Infinity, water);
                buildingCast = Physics.Raycast(t + transform.forward * j, Vector3.down, out hit, Mathf.Infinity, buildingCheck);
                if (!(groundCast && waterCast && !buildingCast))
                {
                    validSpawn = false; break;
                }
            }
            if(validSpawn) // if this is a valid spawn, return it
            {
                return (t);
            }
            
        }
        return (respawnPoints[sizeOfRespawnArray - 1]); // worst case return, hopefully will never get here
    }

    /// <summary>
    /// The new respawning player coroutine. Simplified from the old version, rotates the player 180 degrees and positions them at their respawn point. Also creates a tombstone.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawnPlayer()
    {
        // position of ball
        Vector3 target = GetRespawnPoint();
        Vector3 below = new Vector3(target.x, target.y - liftHeight, target.z);
        Vector3 start = transform.position;
        
        // rotation of control
        Vector3 controlLookat = new Vector3(target.x, control.transform.position.y, target.z) - control.transform.position;
        int rotation = GameManager.Instance.MainState == GameState.FinalPackage ? 0 : 180;
        control.transform.rotation = Quaternion.LookRotation(controlLookat) * Quaternion.Euler(0,rotation,0);
        Quaternion initialRotation = control.transform.rotation;

        orderHandler.DropEverything(target);

        float elapsedTime = 0;
        Instantiate(respawnGravestone, target + control.transform.forward * tombstoneOffset, control.transform.rotation); // spawn respawn gravestone

        // move the player under respawn point and rotate camera
        while (elapsedTime < respawnDuration)
        {
            transform.position = Vector3.Lerp(start, below, elapsedTime / respawnDuration);
            control.transform.rotation = initialRotation * Quaternion.Euler(0,180*(elapsedTime/respawnDuration), 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        target = new Vector3(target.x, target.y + liftHeight, target.z);

        // lift player out of their "tomb"
        while (elapsedTime < liftDuration)
        {
            transform.position = Vector3.Lerp(below, target, elapsedTime / liftDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<SphereCollider>().enabled = true;
        StopRespawnCoroutine();
    }

    private IEnumerator SetRespawnCooldown()
    {
        shouldSetRespawn = false;
        yield return new WaitForSeconds(respawnSetInterval);
        shouldSetRespawn = true;
        respawnSetCooldown = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            // Turning these off fixes camera jittering on respawn
            GetComponent<Rigidbody>().velocity = Vector3.zero; // set velocity to 0 on respawn
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<SphereCollider>().enabled = false;

            StartRespawnCoroutine();
        }
    }

    private void StartRespawnCoroutine()
    {
        if (respawnCoroutine == null)
        {
            respawnCoroutine = RespawnPlayer();
        }
        StartCoroutine(respawnCoroutine);
    }

    private void StopRespawnCoroutine()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }
        respawnCoroutine = null;
    }

    private void StartRespawnCooldown()
    {
        if(respawnSetCooldown == null)
        {
            respawnSetCooldown = SetRespawnCooldown();
        }
        StartCoroutine(respawnSetCooldown);
    }

    private void StopRespawnCooldown()
    {
        if(respawnSetCooldown != null)
        {
            StopCoroutine(respawnSetCooldown);
        }
        shouldSetRespawn = true;
    }

    private void ClearRespawnArray()
    {
        respawnPoints = new Vector3[sizeOfRespawnArray];
        StopRespawnCooldown();
    }
}

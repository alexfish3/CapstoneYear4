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

    private float respawnDuration = 2f;

    [Tooltip("The height to lift the player above the map.")]
    [SerializeField] private float liftHeight = 5.0f; // The height to lift the player above the map

    [Tooltip("The time it takes to lift the player between startingLiftHeight to liftHeight.")]
    [SerializeField] private float liftDuration = 5.0f; // The time it takes to lift the player above the ground

    [Tooltip("Distance between tombstone spawn and player spawn | MUST BE POSITIVE")]
    [SerializeField] private int tombstoneOffset = 2;

    [Tooltip("The prefab for the gravestone model.)")]
    [SerializeField] private GameObject respawnGravestone; // Gravestone model to spawn in during respawn

    [Tooltip("Reference to the control game object.")]
    [SerializeField] private GameObject control;
    private OrderHandler orderHandler;
    private BallDriving ballDriving;

    private Vector3 lastGroundedPos; // last position the player was grounded at
    public Vector3 LastGroundedPos { get { return lastGroundedPos; } set { lastGroundedPos = value; } }

    // Sound stuff
    private SoundPool soundPool;

    // qa
    private QAHandler qa;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapGoldenCutscene += StopRespawnCoroutine;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapGoldenCutscene -= StopRespawnCoroutine;

    }

    private void Start()
    {
        orderHandler = control.GetComponent<OrderHandler>();
        ballDriving = control.GetComponent<BallDriving>();
        soundPool = control.GetComponent<SoundPool>();
        qa = control.GetComponent<QAHandler>();
    }

    /// <summary>
    /// The new respawning player coroutine. Simplified from the old version, rotates the player 180 degrees and positions them at their respawn point. Also creates a tombstone.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawnPlayer()
    {
        RespawnPoint targetPoint = RespawnManager.Instance.GetRespawnPoint(lastGroundedPos);
        Vector3 target = targetPoint.PlayerSpawn;
        Vector3 below = new Vector3(target.x, target.y - liftHeight, target.z);
        Vector3 start = transform.position;
        
        // rotation of control
        Vector3 controlLookat = new Vector3(target.x, control.transform.position.y, target.z) - control.transform.position;
        int rotation = GameManager.Instance.MainState == GameState.FinalPackage ? 0 : 180;
        control.transform.rotation = Quaternion.LookRotation(controlLookat) * Quaternion.Euler(0,rotation,0);
        Quaternion initialRotation = control.transform.rotation;

        orderHandler.DropEverything(targetPoint.Order1Spawn, targetPoint.Order2Spawn, false);

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            qa.Deaths++;
            soundPool.PlayDeathSound();
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
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<SphereCollider>().enabled = true;
    }
}

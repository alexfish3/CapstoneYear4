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

    [Header("Respawn Stats")]
    [Tooltip("The height to lift the player above the map.")]
    [SerializeField] private float liftHeight = 5.0f; // The height to lift the player above the map

    [Tooltip("The time it takes to lift the player between startingLiftHeight to liftHeight.")]
    [SerializeField] private float liftDuration = 5.0f; // The time it takes to lift the player above the ground

    [Tooltip("The height the player wisp will rise above the water.")]
    [SerializeField] private float wispHeight = 5.0f;

    [Tooltip("Time it takes for the wisp to rise above the water.")]
    [SerializeField] private float wispRiseTime = 1f;

    [Tooltip("Time it takes for the wisp to travel to the casket.")]
    [SerializeField] private float wispToCasketTime = 1f;

    [Tooltip("Trailtime of the wisp trail renderer component.")]
    [SerializeField] private float wispTrailTime = 1f;

    [Tooltip("How far below the player death position the wisp spawns.")]
    [SerializeField] private float wispSpawnOffset = 5f;

    [Tooltip("Distance between tombstone spawn and player spawn.")]
    [SerializeField] private int tombstoneOffset = 2;

    [Tooltip("How far below the tombstone the player spawns.")]
    [SerializeField] private float graveDepth = 2f;

    [Header("Game Object Refs")]
    [Tooltip("The prefab for the gravestone model.)")]
    [SerializeField] private GameObject respawnGravestone; // Gravestone model to spawn in during respawn

    [Tooltip("Reference to the control game object.")]
    [SerializeField] private GameObject control;

    [Tooltip("For enabling / disabling the mesh.")]
    [SerializeField] private GameObject modelParent;

    [Tooltip("The trail renderer used to animate the respawn.")]
    [SerializeField] private TrailRenderer respawnWisp;

    private OrderHandler orderHandler;
    private BallDriving ballDriving;

    private Vector3 lastGroundedPos; // last position the player was grounded at
    public Vector3 LastGroundedPos { get { return lastGroundedPos; } set { lastGroundedPos = value; } }

    private Vector3 wispOrigin; // original pos of the wisp

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

        tombstoneOffset = Mathf.Abs(tombstoneOffset); // in case a stinky designer made this value negative
        wispOrigin = respawnWisp.transform.localPosition;

        respawnWisp.time = 0f;
        respawnWisp.gameObject.SetActive(false);

    }

    /// <summary>
    /// The new respawning player coroutine. Simplified from the old version, rotates the player 180 degrees and positions them at their respawn point. Also creates a tombstone.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawnPlayer()
    {
        RespawnPoint rsp = RespawnManager.Instance.GetRespawnPoint(lastGroundedPos); // get the RSP
        rsp.InUse = true;
        // init the wisp
        Vector3 wispStart = respawnWisp.transform.position;
        respawnWisp.transform.position -= Vector3.up * wispSpawnOffset;
        respawnWisp.gameObject.SetActive(true);
        respawnWisp.time = wispTrailTime;

        orderHandler.DropEverything(rsp.Order1Spawn, rsp.Order2Spawn, false);

        float elapsedTime = 0;
        Vector3 wispRise = wispStart + Vector3.up * wispHeight; // position of the wisp risen above water

        // raise the wisp above the water
        while(elapsedTime < wispRiseTime)
        {
            respawnWisp.transform.position = Vector3.Lerp(wispStart, wispRise, elapsedTime / wispRiseTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // reset vars for next movement
        elapsedTime = 0;
        Vector3 casketLocation = rsp.PlayerSpawn - Vector3.up * graveDepth;
        wispStart = respawnWisp.transform.position;

        control.transform.rotation = Quaternion.LookRotation(rsp.PlayerFacingDirection - rsp.PlayerSpawn, Vector3.up);

        Instantiate(respawnGravestone, rsp.PlayerSpawn - control.transform.forward * tombstoneOffset, control.transform.rotation); // spawn respawn gravestone

        // move the wisp to the casket under RSP
        while (elapsedTime < wispToCasketTime)
        {
            respawnWisp.transform.position = Vector3.Lerp(wispStart, casketLocation, elapsedTime / wispToCasketTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // reset vars for next movement
        elapsedTime = 0;
        Vector3 liftTarget = new Vector3(rsp.PlayerSpawn.x, rsp.PlayerSpawn.y + liftHeight, rsp.PlayerSpawn.z);

        // re-enable player mesh and disable wisp
        modelParent.SetActive(true);
        respawnWisp.gameObject.SetActive(false);

        // lift player out of their tomb
        while (elapsedTime < liftDuration)
        {
            transform.position = Vector3.Lerp(casketLocation, liftTarget, elapsedTime / liftDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rsp.InUse = false;
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

        modelParent.SetActive(false);

        StartCoroutine(respawnCoroutine);
    }

    private void StopRespawnCoroutine()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnWisp.time = 0f;
        modelParent.SetActive(true);
        respawnWisp.gameObject.SetActive(false);
        respawnWisp.transform.localPosition = wispOrigin;

        respawnCoroutine = null;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<SphereCollider>().enabled = true;
    }
}

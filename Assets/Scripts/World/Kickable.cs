using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kickable : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;

    private Rigidbody rb;
    public Rigidbody Rb => rb;
    private Dissolver dissolve;

    private bool kicked;
    public bool Kicked => kicked;

    private float fadeTime = 1.0f;

    [Tooltip("Time after being kicked before respawning")]
    [SerializeField] private float respawnWaitTime = 10.0f;

    [Tooltip("Whether this kickable is frozen in place until kicked.")]
    [SerializeField] private bool frozenAtStart = false;

    private IEnumerator respawnCoroutine;
    private IEnumerator checkVelocityCoroutine;

    /// <summary>
    /// Stock Start. Gets references.
    /// </summary>
    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        dissolve = GetComponent<Dissolver>();

        rb = GetComponent<Rigidbody>();

        rb.constraints = frozenAtStart ? RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Kickable" && other.GetComponent<Kickable>().Kicked && other.GetComponent<Kickable>().Rb.velocity.magnitude > 1)
        {
            GetKicked();
        }
    }

    /// <summary>
    /// Public method that can be called when kicked to start the process of fading and respawning.
    /// </summary>
    public void GetKicked(Collider sphereBody = null)
    {
        if (kicked || respawnCoroutine != null)
            return;
        rb.constraints = RigidbodyConstraints.None;

        StopRespawn();
        StartRespawn(sphereBody);
        StopCheckVelocity();
        StartCheckVelocity();

        kicked = true;
    }

    /// <summary>
    /// Waits a duration, fades out, waits for the fade to complete, respawns the object
    /// </summary>
    /// <returns>Boilerplate IEnumerator</returns>
    private IEnumerator Respawn(Collider sphereBody = null)
    {
        yield return new WaitForSeconds(respawnWaitTime);

        dissolve.DissolveOut(fadeTime);

        yield return new WaitForSeconds(fadeTime + 0.5f);

        transform.position = startPos;
        transform.rotation = startRot;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        dissolve.DissolveIn(fadeTime);

        rb.constraints = frozenAtStart ? RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
        kicked = false;
        StopCheckVelocity();

        yield return new WaitForSeconds(fadeTime);

        StopRespawn();
    }

    private IEnumerator CheckVelocity()
    {
        while (true)
        {
            if (rb.velocity.magnitude < 1)
                kicked = false;

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void StartRespawn(Collider sphereBody = null)
    {
        respawnCoroutine = Respawn(sphereBody);
        StartCoroutine(respawnCoroutine);
    }
    private void StopRespawn()
    {
        if (respawnCoroutine != null) 
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    private void StartCheckVelocity()
    {
        checkVelocityCoroutine = CheckVelocity();
        StartCoroutine(checkVelocityCoroutine);
    }
    private void StopCheckVelocity()
    {
        if (checkVelocityCoroutine != null)
        {
            StopCoroutine(checkVelocityCoroutine);
            checkVelocityCoroutine = null;
        }
    }
}

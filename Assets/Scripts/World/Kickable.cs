using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kickable : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;

    private Rigidbody rb;
    private Dissolver dissolve;

    private bool kicked;

    private float fadeTime = 1.0f;

    [Tooltip("Time after being kicked before respawning")]
    [SerializeField] private float respawnWaitTime = 10.0f;

    [Tooltip("Whether this kickable is frozen in place until kicked. IE, whether it can be knocked over by other kickables being flung into it")]
    [SerializeField] private bool frozenAtStart = false;

    private IEnumerator respawnCoroutine;

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
        if (other.tag == "Kickable")
        {
            GetKicked();
        }
    }

    /// <summary>
    /// Public method that can be called when kicked to start the process of fading and respawning.
    /// </summary>
    public void GetKicked(Collider sphereBody = null)
    {
        if (kicked)
            return;
        rb.constraints = RigidbodyConstraints.None;
        StartRespawn(sphereBody);
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
        if (sphereBody != null) 
            Physics.IgnoreCollision(sphereBody, GetComponent<Collider>(), false);

        rb.constraints = frozenAtStart ? RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
        kicked = false;
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kickable : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;

    private Rigidbody rb;
    private Fader fade;

    private float fadeTime = 0.5f;

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
        fade = GetComponent<Fader>();

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
        rb.constraints = RigidbodyConstraints.None;
        StartRespawn(sphereBody);
    }

    /// <summary>
    /// Waits a duration, fades out, waits for the fade to complete, respawns the object
    /// </summary>
    /// <returns>Boilerplate IEnumerator</returns>
    private IEnumerator Respawn(Collider sphereBody = null)
    {
        yield return new WaitForSeconds(respawnWaitTime);

        fade.FadeOut(fadeTime);

        yield return new WaitForSeconds(fadeTime);

        transform.position = startPos;
        transform.rotation = startRot;

        fade.Reset();
        if (sphereBody != null) 
            Physics.IgnoreCollision(sphereBody, GetComponent<Collider>(), false);

        rb.constraints = frozenAtStart ? RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
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

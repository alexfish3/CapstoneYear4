using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kickable : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;

    private Fader fade;

    private float fadeTime = 0.5f;

    [Tooltip("Time after being kicked before respawning")]
    [SerializeField] private float respawnWaitTime = 10.0f;

    private IEnumerator respawnCoroutine;

    /// <summary>
    /// Stock Start. Gets references.
    /// </summary>
    private void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        fade = GetComponent<Fader>();
    }

    /// <summary>
    /// Public method that can be called when kicked to start the process of fading and respawning.
    /// </summary>
    public void GetKicked(Collider sphereBody)
    {
        StartRespawn(sphereBody);
    }

    /// <summary>
    /// Waits a duration, fades out, waits for the fade to complete, respawns the object
    /// </summary>
    /// <returns>Boilerplate IEnumerator</returns>
    private IEnumerator Respawn(Collider sphereBody)
    {
        yield return new WaitForSeconds(respawnWaitTime);

        fade.FadeOut(fadeTime);

        yield return new WaitForSeconds(fadeTime);

        transform.position = startPos;
        transform.rotation = startRot;
        fade.Reset();
        Physics.IgnoreCollision(sphereBody, GetComponent<Collider>(), false);
    }

    private void StartRespawn(Collider sphereBody)
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

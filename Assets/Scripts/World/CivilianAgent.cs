using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

/// <summary>
/// Runs individual pedestrians. They move between a number of points, cycling between them.
/// </summary>
public class CivilianAgent : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Reference to the particle prefab")]
    [SerializeField] private GameObject flashParticles;
    [Tooltip("Reference to the renderers' parent")]
    [SerializeField] private GameObject renderBasket;

    [Header("Values")]
    [Tooltip("How long after being hit it takes for the civilian to reappear")]
    [SerializeField] private float respawnTime = 1.0f;

    private NavMeshAgent agent;
    private float wayPointDistance = 4f;

    private Transform[] points;
    public Transform[] Points { set { points = value; } }

    private int currentIntendedPoint = 0;

    private float slowUpdateTickSpeed = 0.1f; //How frequently, in seconds, SlowUpdate runs
    private IEnumerator slowUpdateCoroutine;

    private IEnumerator deathCoroutine;

    private Transform model;

    private bool moving;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        model = transform.GetChild(0);
    }

    /// <summary>
    /// Can be called externally to begin a pathing routine. Checks that points are loaded, then begins SlowUpdate
    /// </summary>
    public void BeginPathing()
    {
        if (points == null)
        {
            Debug.LogError(this.gameObject.name + " has no assigned patrol points.");
            return;
        }

        agent.SetDestination(points[0].position);

        Tween bob = model.DOLocalMoveY(0.3f, 0.6f, false);
        bob.SetEase(Ease.InOutSine);
        bob.SetLoops(-1, LoopType.Yoyo);

        StartSlowUpdate();
    }

    /// <summary>
    /// Runs a reduced-frequency Update.
    /// </summary>
    /// <returns>Boilerplate IEnumerator</returns>
    private IEnumerator SlowUpdate()
    {
        while (true)
        {
            if (moving && (agent.destination - agent.transform.position).magnitude <= wayPointDistance)
            {
                CyclePoints();
            }

            yield return new WaitForSeconds(slowUpdateTickSpeed);
        }
    }

    /// <summary>
    /// Cycles through the set of points, resetting the count if it reaches the end
    /// </summary>
    private void CyclePoints()
    {
        currentIntendedPoint++;

        if (currentIntendedPoint >= points.Length)
            currentIntendedPoint = 0;

        agent.SetDestination(points[currentIntendedPoint].position);
    }

    /// <summary>
    /// Public method to be called when the ghost should 'die'
    /// Stops it moving, hides the model, calls the particle effect, and starts the respawn timer
    /// </summary>
    public void Die()
    {
        moving = false;
        renderBasket.SetActive(false);

        CreateFlash();
        StartDeath();
    }

    /// <summary>
    /// Waits a duration before reappearing the ghost with another flash
    /// </summary>
    /// <returns>Boilerplate IEnumerator</returns>
    private IEnumerator Death()
    {
        yield return new WaitForSeconds(respawnTime);

        CreateFlash();

        moving = true;
        renderBasket.SetActive(true);
    }

    /// <summary>
    /// Creates a flash particle then destroys it after 1.5 seconds
    /// </summary>
    private void CreateFlash()
    {
        Destroy(Instantiate(flashParticles, transform.position, Quaternion.identity), 1.5f);
    }


    private void StartSlowUpdate()
    {
        slowUpdateCoroutine = SlowUpdate();
        StartCoroutine(slowUpdateCoroutine);
    }
    private void StopSlowUpdate()
    {
        if (slowUpdateCoroutine != null)
        {
            StopCoroutine(slowUpdateCoroutine);
            slowUpdateCoroutine = null;
        }
    }

    private void StartDeath()
    {
        deathCoroutine = Death();
        StartCoroutine(deathCoroutine);
    }
    private void StopDeath()
    {
        if (deathCoroutine != null)
        {
            StopCoroutine(deathCoroutine);
            deathCoroutine = null;
        }
    }
}

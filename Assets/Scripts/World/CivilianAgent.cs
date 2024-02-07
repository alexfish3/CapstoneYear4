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
    private NavMeshAgent agent;
    private float wayPointDistance = 4f;

    [SerializeField]
    private Transform[] points;
    public Transform[] Points { set { points = value; } }

    private int currentIntendedPoint = 0;

    private float slowUpdateTickSpeed = 0.1f; //How frequently, in seconds, SlowUpdate runs
    private IEnumerator slowUpdateCoroutine;

    private Transform model;

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
            if ((agent.destination - agent.transform.position).magnitude <= wayPointDistance)
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
}

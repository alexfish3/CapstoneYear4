using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for the waterfall in the game we want the players to drive though. It enables and disables the collider to achive this effect.
/// </summary>
public class Waterfall : MonoBehaviour
{
    [Tooltip("Starting state of the waterfall")]
    [SerializeField] private bool startOpen = false;
    [Tooltip("Time the waterfall will stay open for")]
    [SerializeField] private float openTime;
    [Tooltip("Time the waterfall will stay closed for")]
    [SerializeField] private float closedTime;
    
    private Collider waterWall;
    private bool canDriveThru;

    private MeshRenderer rend; // temp for visual

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        waterWall = GetComponent<Collider>();
        canDriveThru = startOpen;
        rend.material.color = Color.blue;
        StartWait();
    }

    /// <summary>
    /// Enables or disables the waterwall and starts a coroutine to keep that wall in that state for the appropriate time.
    /// </summary>
    private void StartWait()
    {
        rend.enabled = !canDriveThru;
        waterWall.isTrigger = canDriveThru;
        float waitTime = canDriveThru ? openTime : closedTime;
        StartCoroutine(WaitForChangedState(waitTime));
    }

    /// <summary>
    /// Waits a certain amount of seconds before changing the state of the water wall.
    /// </summary>
    /// <param name="waitTime">Time to wait</param>
    /// <returns></returns>
    private IEnumerator WaitForChangedState(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        canDriveThru = !canDriveThru;
        StartWait();
    }
}

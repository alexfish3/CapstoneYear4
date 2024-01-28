using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] private Transform order1Spawn;
    [SerializeField] private Transform order2Spawn;

    public Vector3 PlayerSpawn { get { return transform.position; } }
    public Vector3 Order1Spawn { get { return order1Spawn.position; } }
    public Vector3 Order2Spawn { get { return order2Spawn.position; } }
    public Vector3 PlayerFacingDirection { get { return playerFacingDirection; } }
    public bool InUse { get { return inUse; } set { inUse = value; } }

    private Vector3 playerFacingDirection;
    private bool inUse = false;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapStartingCutscene += InitPoint;
        GameManager.Instance.OnSwapGoldenCutscene += InitPoint;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapStartingCutscene -= InitPoint;
        GameManager.Instance.OnSwapGoldenCutscene -= InitPoint;
    }

    /// <summary>
    /// This method is for initializing the point, specifially the inUse variable in case the gamestate changes mid-respawn.
    /// </summary>
    public void InitPoint()
    {
        playerFacingDirection = (Order1Spawn + Order2Spawn) / 2f;
        inUse = false;
    }
}

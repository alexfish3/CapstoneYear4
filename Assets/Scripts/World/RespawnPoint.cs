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

    private Vector3 playerFacingDirection;

    public Vector3 PlayerFacingDirection { get { return playerFacingDirection; } }

    private void Start()
    {
        playerFacingDirection = (Order1Spawn + Order2Spawn) / 2f;
    }
}

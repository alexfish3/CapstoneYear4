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
}

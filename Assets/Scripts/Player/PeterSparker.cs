using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeterSparker : SingletonMonobehaviour<PeterSparker>
{
    [Tooltip("Reference to the prefab")]
    [SerializeField] private GameObject impactParticle;

    /// <summary>
    /// Creates sparks on the surface of a collider
    /// </summary>
    /// <param name="input">The collider</param>
    /// <param name="pos">The position of a point in the direction that the sparks should arrive relative to the collider's centre</param>
    public void CreateImpactFromCollider(Collider input, Vector3 pos)
    {
        Vector3 closestPoint = input.ClosestPoint(pos);
        CreateCollisionSparks(closestPoint);
    }

    public void CreateCollisionSparks(Vector3 sparkPoint)
    {
        GameObject hit = Instantiate(impactParticle, sparkPoint, Quaternion.identity);
        Destroy(hit, 1.5f);
    }
}

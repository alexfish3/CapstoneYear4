using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeterSparker : SingletonMonobehaviour<PeterSparker>
{
    [Tooltip("Reference to the prefab")]
    [SerializeField] private GameObject impactParticle;

    /// <summary>
    /// Creates sparks at the point of impact between two colliders. The class calling the script should pass reference to the *other* collider
    /// </summary>
    /// <param name="input">The *other* collider in the collision</param>
    /// <param name="selfPosition">The position of the object calling for particles</param>
    public void CreateImpactFromCollider(Collider input, Transform selfPosition)
    {
        Vector3 closestPoint = input.ClosestPoint(selfPosition.position);
        CreateCollisionSparks(closestPoint);
    }

    public void CreateCollisionSparks(Vector3 sparkPoint)
    {
        GameObject hit = Instantiate(impactParticle, sparkPoint, Quaternion.identity);
        Destroy(hit, 1.5f);
    }
}

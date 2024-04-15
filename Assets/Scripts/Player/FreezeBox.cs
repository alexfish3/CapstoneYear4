using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Freezes a player when they enter this trigger. Used for the results cutscene.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class FreezeBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BallDriving bd;

        try
        {
            bd = other.GetComponent<BallDriving>();
            bd.FreezeBall(true, false);
        }
        catch
        {
            return;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBuilding : MonoBehaviour
{
    public float maxBreakTime = 5.0f;
    public float breakTimer;
    private bool isBroken = false;

    private MeshRenderer mesh;
    private BoxCollider boxCollider;

    private void Start()
    {
        breakTimer = maxBreakTime;
        mesh = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            ScooterMovement player = other.GetComponent<ScooterMovement>();
            if(player.boosting && !isBroken)
            {
                breakBuilding();
                isBroken = true;
            }
        }
    }
    private IEnumerator breakBuilding()
    {
        mesh.enabled = false;
        boxCollider.enabled = false;
        breakTimer -= Time.deltaTime;
        if(breakTimer > 0)
        {
            yield return null;
        }
        else
        {
            Rebuild();
        }
    }
    private void Rebuild()
    {
        breakTimer = maxBreakTime;
        mesh.enabled = true;
        boxCollider.enabled = true;
    }
}

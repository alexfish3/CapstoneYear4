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
                StartCoroutine(breakBuilding());
                isBroken = true;
            }
        }
    }

    private IEnumerator breakBuilding()
    {
        mesh.enabled = false;
        boxCollider.enabled = false;
        
        yield return new WaitForSeconds(breakTimer);

        Rebuild();
    }
    private void Rebuild()
    {
        breakTimer = maxBreakTime;
        mesh.enabled = true;
        boxCollider.enabled = true;
        isBroken = false;
    }
}

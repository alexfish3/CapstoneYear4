using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Value
{
    Easy = 10,
    Medium = 20,
    Hard = 30
}
public class Package : MonoBehaviour
{
    private ScooterMovement playerHolding;
    public bool isHolding;
    [SerializeField] GameObject model;
    [SerializeField] GameObject pickup;
    [SerializeField] GameObject dropOff;
    [SerializeField] GameObject beacon;
    [SerializeField] private Value value;
    private BoxCollider hitBox;
    
    private float distance;
    private float points;

    private void Start()
    {
        distance = Mathf.Abs(Vector3.Distance(pickup.transform.position, dropOff.transform.position));
        points = distance + (int)value;
        initBeacon();
        hitBox = GetComponent<BoxCollider>();
    }
    public void Pickup(ScooterMovement player)
    {
        if (!isHolding)
        {
            playerHolding = player;
            beacon.transform.position = dropOff.transform.position;
            model.gameObject.transform.parent = playerHolding.transform;
            // some method to assign THIS to player
            isHolding = true;
        }
    }
    public void Stolen(ScooterMovement playerStealing)
    {
        // some method to assign this package to new player
        playerHolding = playerStealing;
    }
    public void DropOff()
    {
        // give the player the score (should be a method on the player)
        // unassign this to player
        Destroy(model);
        Destroy(this);
    }
    private void initBeacon()
    {
        MeshRenderer beaconMesh = beacon.GetComponent<MeshRenderer>();
        beacon.transform.position = pickup.transform.position;
        switch(value)
        {
            case Value.Easy:
                beaconMesh.material.color = Color.green;
                break;
            case Value.Medium:
                beaconMesh.material.color = Color.yellow;
                break;
            case Value.Hard:
                beaconMesh.material.color = Color.red;
                break;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Pickup(other.GetComponent<ScooterMovement>());
        }
    }
}

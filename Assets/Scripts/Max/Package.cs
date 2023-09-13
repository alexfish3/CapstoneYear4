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
    [HideInInspector] public float points;

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
            player.PickupPackage(this);
            isHolding = true;
        }
    }
    public void Stolen(ScooterMovement playerStealing)
    {
        playerHolding.DropPackage(this, false);
        playerStealing.PickupPackage(this);
        playerHolding = playerStealing;
    }
    public void DropOff()
    {
        playerHolding.DropPackage(this, true);
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

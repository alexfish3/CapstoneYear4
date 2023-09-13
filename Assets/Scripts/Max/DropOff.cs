using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropOff : MonoBehaviour
{
    [SerializeField] private Package package;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Drop Off");
        if(other.tag == "Player")
        {
            ScooterMovement player = other.GetComponent<ScooterMovement>();
            if(package.playerHolding == player)
            {
                package.Deliver();
            }
        }
    }
}

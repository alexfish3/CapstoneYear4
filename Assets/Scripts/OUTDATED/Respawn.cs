using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Respawn : MonoBehaviour
{
    //Variables
    Vector3 firstPoint;
    Vector3 secondPoint;
    Vector3 respawnPoint;




    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            secondPoint = gameObject.transform.position;
            //Debug.Log(secondPoint + " second");
            // Commenting this as this currently causes issues with respawning player while grabbiung package
            //RespawnPlayer();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "RespawnCollider")
        {
            firstPoint = gameObject.transform.position;
        }
    }


    public void RespawnPlayer()
    {
        Debug.Log("RESAWPN PLAYER");


        /*
        //Old stuff but keeping for some reason idk
        Vector3 direction = (secondPoint - firstPoint).normalized;
        respawnPoint = firstPoint - (direction * 6);
        respawnPoint.y = 4;
        */


        respawnPoint = firstPoint;

        //Debug.Log(respawnPoint + " respawn");
        gameObject.transform.position = respawnPoint;

    }
}

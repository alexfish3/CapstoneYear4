using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Respawn : MonoBehaviour
{
    //Variables
    Vector3 respawnPoint;




    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            RespawnPlayer();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //save player location before they drive off land
        if (other.tag == "RespawnCollider")
        {
            respawnPoint = gameObject.transform.position;
        }
    }


    public void RespawnPlayer()
    {
        Debug.Log("RESAWPN PLAYER");

        //Debug.Log(respawnPoint + " respawn");

        //teleport player back onto land
        gameObject.transform.position = respawnPoint;

    }
}

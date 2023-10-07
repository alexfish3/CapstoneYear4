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
        if (other.tag == "Collider")
        {
            firstPoint = gameObject.transform.position;
            //Debug.Log(firstPoint + " first");
        }
        if (other.tag == "Water")
        {
            secondPoint = gameObject.transform.position;
            //Debug.Log(secondPoint + " second");
            RespawnPlayer();
        }
    }


    public void RespawnPlayer()
    {
        Vector3 direction = (secondPoint - firstPoint).normalized;
        respawnPoint = firstPoint - (direction * 16);
        respawnPoint.y = 4;

        //Debug.Log(respawnPoint + " respawn");
        gameObject.transform.position = respawnPoint;

    }
}

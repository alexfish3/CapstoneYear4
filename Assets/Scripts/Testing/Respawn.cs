using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Respawn : MonoBehaviour
{
    //Variables
    Vector3 firstPoint;
    Vector3 secondPoint;




    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Collider")
        {
            firstPoint = gameObject.transform.position;
        }
        if (other.tag == "Water")
        {
            secondPoint = gameObject.transform.position;
            RespawnPlayer();
        }
    }


    public void RespawnPlayer()
    {
        gameObject.transform.position = ((secondPoint - firstPoint).normalized) * -1;

    }
}

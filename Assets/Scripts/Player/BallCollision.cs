using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class deals with collisions on the Ball of Fun and calls various methods from the player control scripts.
/// </summary>
public class BallCollision : MonoBehaviour
{
    private OrderHandler orderHandler;
    private Respawn respawnHandler;

    private void Start()
    {
        orderHandler = transform.parent.GetComponentInChildren<OrderHandler>();
        respawnHandler = transform.parent.GetComponentInChildren<Respawn>();
    }

/*    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<BallCollision>() != null)
        {
            orderHandler.Collision(collision.gameObject.GetComponent<BallCollision>());
        }
    }*/

/*    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water") // player falls in water
        {
            respawnHandler.RespawnPlayer(this.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "RespawnCollider") // player exits respawn collidier (falls off the edge)
        {
            respawnHandler.SetRespawnPoint();
        }
    }*/
}

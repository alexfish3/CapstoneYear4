using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollision : MonoBehaviour
{
    private OrderHandler orderHandler;

    public event Action OnBallCollision;

    private void Start()
    {
        orderHandler = transform.parent.GetComponentInChildren<OrderHandler>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<BallCollision>() != null)
        {
            orderHandler.Collision(collision.gameObject.GetComponent<BallCollision>());
        }

    }
}

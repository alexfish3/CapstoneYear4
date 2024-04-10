using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for the cardboard cutout in the tutorial.
/// </summary>
public class CutoutHandler : MonoBehaviour
{
    [Tooltip("Model of the cutout for spinning and whatnot.")]
    [SerializeField] private Transform cutoutModel;

    [Tooltip("Order the cutout will be holding.")]
    [SerializeField] private Order order;

    [Tooltip("Position the order will be.")]
    [SerializeField] private Transform orderPos;

    [Tooltip("GO to block the player from advancing until they've stolen the order.")]
    [SerializeField] private GameObject barrier;

    [Tooltip("Time it takes for the barrier to dissolve.")]
    [SerializeField] private float dissolveTime = 0.5f;

    private Dissolver barrierDissolver;
    private BoxCollider barrierCollider;
    private BoxCollider cutoutMeshCollider;

    private bool hasStolen = false;

    public void InitCutout()
    {
        order.CardboardHold();
        order.transform.position = orderPos.position;

        barrierDissolver = barrier.GetComponent<Dissolver>();
        barrierCollider = barrier.GetComponent<BoxCollider>();
        cutoutMeshCollider = cutoutModel.GetComponent<BoxCollider>();

        barrierCollider.enabled = true;
    }

    /// <summary>
    /// Gives a spin animation with DOTween to simulate being stolen from by a player.
    /// </summary>
    private void SpinCutout(float tweenTime)
    {
        cutoutMeshCollider.enabled = false;
        Tween spinning = cutoutModel.DORotate(new Vector3(cutoutModel.rotation.x, 360, cutoutModel.rotation.z), tweenTime, RotateMode.LocalAxisAdd);
        spinning.SetEase(Ease.OutBack); //an easing function which dictates a steep climb, slight overshoot, then gradual correction
        spinning.onComplete += () => cutoutMeshCollider.enabled = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasStolen)
            return;

        OrderHandler player;
        BallDriving playerBall;

        try
        {
            player = other.gameObject.transform.parent.GetComponentInChildren<OrderHandler>();
            playerBall = other.gameObject.transform.parent.GetComponentInChildren<BallDriving>();
            SoundPool sp = player.GetComponent<SoundPool>();
            if (playerBall.Boosting)
            {
                order.StealActive = false;
                order.InitOrder(false);
                player.AddOrder(order);
                hasStolen = true;
                barrierCollider.enabled = false;
                barrierDissolver.DissolveOut(dissolveTime);
                SpinCutout(1f);
                sp.PlayOrderTheft();
            }
        }
        catch
        {
            return;
        }
    }
}

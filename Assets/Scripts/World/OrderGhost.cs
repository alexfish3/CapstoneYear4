using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the class controlling the ghost that will receive the order at the dropoff point.
/// </summary>
public class OrderGhost : MonoBehaviour
{
    private bool startedWaiting = false;
    private bool delivered = false;
    private Tween walkingTween;

    [Tooltip("Time it takes to walk in a full circle.")]
    [SerializeField] private float walkTime = 1f;
    [Tooltip("For the small rotation when the customer receieves their order before they die.")]
    [SerializeField] private float happyTime = 1f;
    [Tooltip("Reference to the mesh the order is delivered to.")]
    [SerializeField] private GameObject mesh;
    public Vector3 CustomerPos { get { return mesh.transform.position; } }

    // Start is called before the first frame update
    void Start()
    {
        DOTween.Kill(transform);
        this.gameObject.SetActive(false);
        startedWaiting = false;
        delivered = false;
        walkingTween = null;
    }

    /// <summary>
    /// Called when the order beacon is created. Creates the customer and starts their walk.
    /// </summary>
    public void InitCustomer()
    {
        if (startedWaiting || delivered) { return; }
        startedWaiting = true;
        this.gameObject.SetActive(true);
        walkingTween = transform.DORotate(new Vector3(0, -360f, 0), walkTime, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetRelative()
            .SetEase(Ease.Linear);
    }

    /// <summary>
    /// Is called when the player enters the beacon. Kills the walking tween so that the customer can stand still and receive their order.
    /// </summary>
    public void DeliveredOrder()
    {
        if(walkingTween != null) 
        {
            walkingTween.Kill();
            walkingTween = null;
        }
        delivered = true;
    }

    /// <summary>
    /// This method called when the customer's order is delivered to them. It will play some happy animation and then reset the customer.
    /// </summary>
    public void ThankYouComeAgain()
    {
        mesh.transform.DORotate(new Vector3(mesh.transform.rotation.x, mesh.transform.rotation.y + 360f, mesh.transform.rotation.z), happyTime, RotateMode.FastBeyond360)
            .SetRelative().OnComplete(() => { Start(); });
    }
}

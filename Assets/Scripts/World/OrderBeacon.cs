using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// This class is for the beacon to indicate where an order is and where it needs to be delivered to
/// </summary>
public class OrderBeacon : MonoBehaviour
{
    private Order order; // order the beacon is tracking
    private bool isPickup = true;
    public bool IsPickup { get { return isPickup; } }

    [SerializeField] private MeshRenderer meshRenderer; // renderer to change the color
    private Color color;

    [SerializeField] private CompassMarker compassMarker; // for the dropoff location on the compass marker
    public CompassMarker CompassMarker { get { return compassMarker; } }

    [SerializeField] private OrderGhost customer;
    [SerializeField] private MeshRenderer dissolveRend;
    [SerializeField] private Material[] dissolveMats;
    [SerializeField] private Material[] pickupMats;
    [SerializeField] private Material dropoffMat;

    /// <summary>
    /// This method initializes the beacon. It sets the order the beacon is tracking, sets the color, and the position.
    /// </summary>
    /// <param name="</param>
    public void InitBeacon(Order inOrder, int orderIndex)
    {
        ToggleBeaconMesh(true);
        order = inOrder;
        meshRenderer.material = pickupMats[orderIndex];
        this.transform.position = order.transform.position;

        // set the color of the dissolve
        switch(order.Value)
        {
            case Constants.OrderValue.Easy:
                dissolveRend.material = dissolveMats[0];
                break;
            case Constants.OrderValue.Medium:
                dissolveRend.material = dissolveMats[1];
                break;
            case Constants.OrderValue.Hard:
                dissolveRend.material = dissolveMats[2];
                break;
            case Constants.OrderValue.Golden:
                dissolveRend.material = dissolveMats[3];
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// This method changes the pickup beacon to a dropoff beacon.
    /// </summary>
    /// <param name="dropoff">Dropoff location</param>
    public void SetDropoff(Transform dropoff)
    {
        this.transform.position = dropoff.position;
        this.transform.parent = OrderManager.Instance.transform;
        
        customer.transform.position = dropoff.position;
        customer.transform.parent = this.transform;
        customer.InitCustomer();
        
        isPickup = false;
        meshRenderer.material = dropoffMat;

        compassMarker.RemoveCompassUIFromAllPlayers();

        order.PlayerHolding.GetComponent<Compass>().AddCompassMarker(compassMarker);
        // NOTE: if camera layers change it'll fuck with beacon rendering
        gameObject.layer = order.PlayerHolding.transform.parent.GetComponentInChildren<SphereCollider>().gameObject.layer + 7;
    }

    /// <summary>
    /// This method will set the beacon back to a pickup beacon in the event it is knocked out of the player's possession.
    /// </summary>
    public void ResetPickup()
    {
        compassMarker.RemoveCompassUIFromAllPlayers();
        order.compassMarker.InitalizeCompassUIOnAllPlayers();

        customer.transform.parent = OrderManager.Instance.transform;
        this.transform.position = order.transform.position;
        ToggleBeaconMesh(true);
        meshRenderer.material.color = color;
        isPickup = true;
        order.RemovePlayerHolding();
        gameObject.layer = 0; // reset to default layer
    }

    /// <summary>
    /// Sets the mesh renderer of the beacon to true or false. Used for hiding the beacon during a drop animation.
    /// </summary>
    /// <param name="status">Enabled property of the mesh renderer</param>
    public void ToggleBeaconMesh(bool status)
    {
        meshRenderer.enabled = status;
        dissolveRend.enabled = status;
    }

    /// <summary>
    /// This method is the first half of the process to reset the beacon. It "throws" the order to the customer for them to catch.
    /// </summary>
    public void ThrowOrder(float throwTime)
    {
        ToggleBeaconMesh(false);
        gameObject.layer = 0;

        customer.transform.parent = OrderManager.Instance.transform;
        customer.DeliveredOrder();
        order.transform.DOMove(customer.CustomerPos, throwTime).OnComplete(() => { EraseBeacon(); });
    }

    /// <summary>
    /// This method is the second half of the process to reset the beacon. It is called when the customer "catches" the order.
    /// </summary>
    public void EraseBeacon()
    {
        Debug.Log("Erase Becon");

        gameObject.GetComponent<CompassMarker>().RemoveCompassUIFromAllPlayers();

        customer.ThankYouComeAgain();
        ToggleBeaconMesh(false);
        gameObject.layer = 0;

        if (order != null)
        {
            order.EraseOrder();
        }
        isPickup = true;
    }

    /// <summary>
    /// Basic OnTriggerEnter, will execute whenever something enters the beacon's light.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Ball Of Fun")
        {
            Transform parent = other.transform.parent;
            OrderHandler orderHandler = parent.GetComponentInChildren<OrderHandler>();
            if (isPickup)
            {
                orderHandler.AddOrder(order); // add the order if the beacon is a pickup beacon
            }
            else
            {
                orderHandler.DeliverOrder(order); // deliver the order if the beacon is a dropoff beacon
            }
        }
    }
}

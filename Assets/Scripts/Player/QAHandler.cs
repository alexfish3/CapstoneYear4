using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for gathering player information and sending it to the QA manager. Attach to control on player prefab.
/// </summary>
public class QAHandler : MonoBehaviour
{
    private OrderHandler orderHandler;
    private float easy = 0, med = 0, hard = 0, gold = 0; // number of each package type delivered

    private void Start()
    {
        orderHandler = GetComponent<OrderHandler>();
        QAManager.Instance.AddHandler(this);
    }

    public void ResetQA()
    {
        easy = 0;
        med = 0;
        hard = 0;
        gold = 0;
    }

    public string GetData()
    {
        string data = this.gameObject.transform.parent.name + ":\n";
        data += $"Placement: {orderHandler.Placement} | Score: ${orderHandler.Score}\n";
        data += $"Easy: {easy} | Medium: {med} | Hard: {hard} | Gold: {gold}\n";
        return data;
    }

    public void Deliver(Constants.OrderValue value)
    {
        switch(value)
        {
            case Constants.OrderValue.Easy:
                easy++;
                break;
            case Constants.OrderValue.Medium:
                med++;
                break;
            case Constants.OrderValue.Hard: 
                hard++; 
                break;
            case Constants.OrderValue.Golden:
                gold++; 
                break;
        }
    }
}

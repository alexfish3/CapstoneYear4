using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is for gathering player information and sending it to the QA manager. Attach to control on player prefab.
/// </summary>
public class QAHandler : MonoBehaviour
{
    private OrderHandler orderHandler;
    
    // raw stats
    private int easy = 0, med = 0, hard = 0, gold = 0; // number of each package type delivered

    private int boosts=0, steals=0, deaths=0; // counters for different gameplay functions

    public int Boosts { get { return boosts; } set {  boosts = value; } }
    public int Steals { get { return steals; } set {  steals = value; } }
    public int Deaths { get { return deaths; } set {  deaths = value; } }

    // heat map
    private GameObject trailObject;
    [SerializeField] private float trailFrequency = 1f;
    private Color trailColor;
    private bool shouldTrail = false;
    private float trailTimer = 0f;
    private float iconHeight = 50f;
    [SerializeField] private GameObject deathIcon;

    private void Start()
    {
        orderHandler = GetComponent<OrderHandler>();
        QAManager.Instance.AddHandler(this);
    }

    private void Update()
    {
        if (!shouldTrail) { return; }

        if(trailTimer > trailFrequency)
        {
            Instantiate(trailObject, this.transform.position + iconHeight * transform.up, 
                this.transform.rotation * Quaternion.Euler(90,1,1), this.transform.parent);
            trailTimer = 0f;
        }

        trailTimer += Time.deltaTime;
    }

    public void SetTrailObj(GameObject inTrail)
    {
        trailObject = inTrail;
        shouldTrail = true;
    }

    public void SetDeath()
    {
        Instantiate(deathIcon, transform.position + iconHeight * transform.up, 
            transform.rotation * Quaternion.Euler(90,1,1), this.transform.parent);
    }

    public void ResetQA()
    {
        easy = 0;
        med = 0;
        hard = 0;
        gold = 0;
    }

    public string[] GetData()
    {
        string[] data = { 
            this.gameObject.transform.parent.name,
            orderHandler.Placement.ToString(),
            orderHandler.Score.ToString(),
            easy.ToString(), med.ToString(), hard.ToString(), gold.ToString(), OrderManager.Instance.FinalOrderValue.ToString(),
            deaths.ToString(), boosts.ToString(), steals.ToString()
        };

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

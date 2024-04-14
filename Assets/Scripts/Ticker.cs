using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ticker : MonoBehaviour
{
    public static float tickTime_010 = 0.1f;
    private float tickerTimer_010;

    public delegate void TickAction010();
    public static event TickAction010 OnTickAction010;

    // Update is called once per frame
    void Update()
    {
        tickerTimer_010 += Time.deltaTime;

        if(tickerTimer_010 >= tickTime_010)
        {
            tickerTimer_010 = 0;
            TickEvent();
        }
    }

    private void TickEvent()
    {
        OnTickAction010?.Invoke();
    }
}

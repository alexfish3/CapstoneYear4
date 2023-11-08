using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drawbridge : MonoBehaviour
{
    [SerializeField] private GameObject Left;
    [SerializeField] private GameObject Right;

    private int seconds;
    private int time;

    // Start is called before the first frame update
    void Start()
    {
        seconds = 9000;
        time = 180;
        StartCoroutine(Draw());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Draw()
    {
        while (seconds != 0f)
        {
            seconds = seconds - 1;
            Left.transform.Rotate(0.5f, 0.0f, 0.0f);
            Right.transform.Rotate(-0.5f, 0.0f, 0.0f);

            yield return new WaitForSeconds(0.5f);
            seconds = 180;
            StartCoroutine(Timer());
        }



    }

    IEnumerator Timer()
    {
        while (time != 0f)
        {
            time = time - 1;

            yield return new WaitForSeconds(1f);
        }



    }

}

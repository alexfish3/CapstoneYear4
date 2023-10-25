using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField]
    private int seconds;

    [SerializeField]
    private GameObject Barrier01;
    [SerializeField]
    private GameObject Barrier02;
    [SerializeField]
    private GameObject Barrier03;
    [SerializeField]
    private GameObject Barrier04;

    [SerializeField]
    private GameObject Barrier05;
    [SerializeField]
    private GameObject Barrier06;
    [SerializeField]
    private GameObject Barrier07;
    [SerializeField]
    private GameObject Barrier08;

    private bool enable;

    // Start is called before the first frame update
    void Start()
    {
        seconds = 10;

        Barrier01.SetActive(true);
        Barrier02.SetActive(true);
        Barrier03.SetActive(false);
        //Barrier04.SetActive(false);

        //Barrier05.SetActive(true);
        //Barrier06.SetActive(true);
        //Barrier07.SetActive(true);
        //Barrier08.SetActive(false);





        enable = true;

        StartCoroutine(Timer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void move()
    {

        if(enable == true) { 
            Barrier01.SetActive(false);
            Barrier02.SetActive(false);
            Barrier03.SetActive(true);
            //Barrier04.SetActive(true);

           // Barrier05.SetActive(false);
            //Barrier06.SetActive(false);
            //Barrier07.SetActive(false);
            //Barrier08.SetActive(true);

            enable = false;

        }
        else
        {
            Barrier01.SetActive(true);
            Barrier02.SetActive(true);
            Barrier03.SetActive(false);
            //Barrier04.SetActive(false);

            //Barrier05.SetActive(true);
            //Barrier06.SetActive(true);
           // Barrier07.SetActive(true);
            //Barrier08.SetActive(false);

            enable = true;
        }
        seconds = 10;
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {
        while(seconds != 0)
        {
            seconds--;

            yield return new WaitForSeconds(1f);
        }
        
        move();
    }
}

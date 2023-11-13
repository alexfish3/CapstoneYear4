using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTimer : MonoBehaviour
{
    [SerializeField]
    private int seconds;

    [SerializeField]
    private GameObject UI;

    private int time;


    // Start is called before the first frame update
    void Start()
    {
        time = seconds;
        UI.SetActive(true);
        StartCoroutine(Timer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator Timer()
    {
        while (time != 0)
        {
            time--;

            yield return new WaitForSeconds(1f);
        }

        UI.SetActive(false);

    }
}

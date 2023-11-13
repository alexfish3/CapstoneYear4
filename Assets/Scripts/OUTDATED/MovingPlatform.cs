using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UIElements;

public class MovingPlatform : MonoBehaviour
{
    //Variables
    [SerializeField] private Transform startingPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private float speed = 1.0f;
    private Transform temp = null;

    [SerializeField]
    private bool waitAtEnd = false;

    [SerializeField]
    private float secondsToWaitFor = 1;

    private void Start()
    {
        gameObject.transform.position = startingPos.position;
        StartCoroutine(PauseAtEndPoint());
    }

    private IEnumerator PauseAtEndPoint()
    {
        while (true)
        {
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, endPos.position, speed * Time.deltaTime);

            if (transform.position == endPos.position)
            {
                temp = startingPos;
                startingPos = endPos;
                endPos = temp;

                if (waitAtEnd)
                {
                    yield return new WaitForSecondsRealtime(secondsToWaitFor);
                }
            }

            yield return null;
        }

    }


}

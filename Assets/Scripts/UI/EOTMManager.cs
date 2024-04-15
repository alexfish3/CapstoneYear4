using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EOTMManager : MonoBehaviour
{
    [SerializeField] private GameObject[] frameGOs;

    private void OnEnable()
    {
        //GameManager.Instance.OnSwapResults += InitFrames;
    }

    private void OnDisable()
    {
        //GameManager.Instance.OnSwapResults -= InitFrames;
    }

    private void InitFrames()
    {
        for(int i=0;i<PlayerInstantiate.Instance.PlayerCount;i++)
        {
            frameGOs[i].SetActive(true);
        }
    }
}

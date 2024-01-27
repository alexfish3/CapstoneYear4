using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : SingletonMonobehaviour<RespawnManager>
{
    [Tooltip("Parent Game Object for the respawn points of the main map.")]
    [SerializeField] private GameObject mainMapRSPParent;

    [Tooltip("Parent Game Object for the respawn points of the final order sequence map.")]
    [SerializeField] private GameObject finalMapRSPParent;

    [Tooltip("Will return any respawn point less than this distance, even if it's not technically the closest.")]
    [SerializeField] private float closeEnough = 10f;

    private RespawnPoint[] respawnPoints;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapBegin += () => InitRespawnPoints(mainMapRSPParent);
        GameManager.Instance.OnSwapGoldenCutscene += () => InitRespawnPoints(finalMapRSPParent);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapBegin -= () => InitRespawnPoints(mainMapRSPParent);
        GameManager.Instance.OnSwapGoldenCutscene -= () => InitRespawnPoints(finalMapRSPParent);
    }

    private void InitRespawnPoints(GameObject parent)
    {
        respawnPoints = new RespawnPoint[parent.transform.childCount];
        for(int i=0;i<parent.transform.childCount;i++)
        {
            respawnPoints[i] = parent.transform.GetChild(i).GetComponent<RespawnPoint>();
        }
    }

    public RespawnPoint GetRespawnPoint(Vector3 lastGrounded)
    {
        if(respawnPoints.Length == 0)
        {
            Debug.LogError("No respawn points in array!");
            return null;
        }

        float minDist = Vector3.Distance(lastGrounded, respawnPoints[0].PlayerSpawn);
        int rspIndex = 0;
        for(int i=1;i<respawnPoints.Length;i++)
        {
            float newDist = Vector3.Distance(lastGrounded, respawnPoints[i].PlayerSpawn);
            if (newDist < minDist)
            {
                if(newDist < closeEnough) { return respawnPoints[i]; }
                rspIndex = i;
                minDist = newDist;
            }
        }
        return respawnPoints[rspIndex];
    }
}

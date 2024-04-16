using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultsMovementController : MonoBehaviour
{
    private List<GameObject> players = new List<GameObject>();
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private GameObject enableThisForResults;
    [SerializeField] private float timePerBoost = 1f;
    [SerializeField] private Collider fountainCollider;

    private void OnEnable()
    {
        //GameManager.Instance.OnSwapGoldenCutscene += () => enableThisForResults.SetActive(false);
        GameManager.Instance.OnSwapResults += SpawnPlayers;
    }
    private void OnDisable()
    {
       // GameManager.Instance.OnSwapGoldenCutscene -= () => enableThisForResults.SetActive(false);
        GameManager.Instance.OnSwapResults -= SpawnPlayers;
    }

    private void SortPlayers()
    {
        players.Clear();
        for(int i=0;i<PlayerInstantiate.Instance.PlayerCount;i++)
        {
            try
            {
                players.Add(ScoreManager.Instance.GetHandlerOfIndex(i).transform.parent.GetComponentInChildren<Respawn>().gameObject); // super scuffed way to get ref to the sphere
            }
            catch
            {
                continue;
            }
        }
    }

    private void SpawnPlayers()
    {
        SortPlayers();
        enableThisForResults.SetActive(true);
        for(int i=0;i<players.Count;i++)
        {
            players[i].transform.position = playerSpawns[i].position;
            players[i].transform.rotation = playerSpawns[i].rotation;
        }
    }
}

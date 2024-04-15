using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultsMovementController : SingletonMonobehaviour<ResultsMovementController>
{
    private List<BallDriving> players = new List<BallDriving>();
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private GameObject enableThisForResults;
    [SerializeField] private float timePerBoost = 1f;
    [SerializeField] private Collider fountainCollider;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapGoldenCutscene += () => enableThisForResults.SetActive(false);
        GameManager.Instance.OnSwapResults += () => fountainCollider.enabled = false;
        GameManager.Instance.OnSwapGoldenCutscene += () => fountainCollider.enabled = true;
        GameManager.Instance.OnSwapResults += SpawnPlayers;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnSwapGoldenCutscene -= () => enableThisForResults.SetActive(false);
        GameManager.Instance.OnSwapResults -= () => fountainCollider.enabled = false;
        GameManager.Instance.OnSwapGoldenCutscene -= () => fountainCollider.enabled = true;

        GameManager.Instance.OnSwapResults -= SpawnPlayers;
    }

    public void AddPlayer(BallDriving inPlayer)
    {
        if (!players.Contains(inPlayer))
            players.Add(inPlayer);
    }

    public void BoostPlayer(int placement)
    {
        placement--; // for zero indexing
        try
        {
            players[placement].AutoBoost();
        }
        catch 
        {
            return;
        }
    }

    private void SortPlayers()
    {
        players.Clear();
        for(int i=0;i<PlayerInstantiate.Instance.PlayerCount;i++)
        {
            try
            {
                players.Add(ScoreManager.Instance.GetHandlerOfIndex(i).GetComponent<BallDriving>());
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
        StartCoroutine(BoostingRoutine());
    }

    private IEnumerator BoostingRoutine()
    {
        for(int i=0;i<players.Count;i++)
        {
            players[i].AutoBoost();
            yield return new WaitForSeconds(timePerBoost);
        }
    }
}

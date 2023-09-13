using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<ScooterMovement> playerList;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void AddPlayer(ScooterMovement player)
    {
        if (!playerList.Contains(player))
        {
            playerList.Add(player);
            player.name = "Player " + playerList.Count;
        }
    }
    public void GameOver()
    {
        SceneManager.LoadScene("End Screen", LoadSceneMode.Additive);
    }
}

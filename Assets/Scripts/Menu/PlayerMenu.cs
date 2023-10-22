using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMenu : SingletonMonobehaviour<PlayerMenu>
{
    [SerializeField] PlayerInstantiate playerInstantiate;

    private void OnEnable()
    {
        playerInstantiate.OnReadiedUp.AddListener(LoadGame);
    }

    private void OnDisable()
    {
        playerInstantiate.OnReadiedUp.RemoveListener(LoadGame);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerInstantiate.CheckReadyUpCount();
        }
    }

    ///<summary>
    /// Main method that loads the game
    ///</summary>
    private void LoadGame()
    {
        SceneManager.LoadScene(1);
    }

}

using System.Collections;
using Udar.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] float timer;
    Coroutine sceneLoadCoroutune;
    [SerializeField] SceneField PlayerSelectScene;
    [SerializeField] Animator splashScreenAnim;

    public void Start()
    {
        LoadMenuFromSplash(timer);
    }

    ///<summary>
    /// Main method that loads the menu from splashscreen
    ///</summary>
    public void LoadMenuFromSplash(float timer)
    {
        // Stops Corutine
        if (sceneLoadCoroutune != null)
        {
            StopCoroutine(sceneLoadCoroutune);
            sceneLoadCoroutune = null;
        }

        sceneLoadCoroutune = StartCoroutine(LoadSceneAsync(PlayerSelectScene.BuildIndex, timer));
    }

    ///<summary>
    /// Loads the scene async
    ///</summary>
    private IEnumerator LoadSceneAsync(int sceneToLoad, float delayTime)
    {
        // Sets gamestate to loading

        // Loads the first scene asynchronously
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene is allowed to be activated
        while (!asyncLoad.allowSceneActivation)
        {
            if (asyncLoad.progress >= 0.90f && splashScreenAnim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
            {
                yield return new WaitForSeconds(delayTime);

                asyncLoad.allowSceneActivation = true;

                break;
            }
            yield return null;
        }
    }

}

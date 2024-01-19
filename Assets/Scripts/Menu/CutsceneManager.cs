using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    enum CutsceneType
    {
        StartingCutscene,
        FinalOrderCutscene
    }
    [SerializeField] GameObject[] cameraPositions;
    [SerializeField] Camera cutsceneCamera;
    [SerializeField] Canvas cutsceneCanvas;
    [SerializeField] PlayableDirector playableDirector;

    [Header("Cutscene Information")]
    Coroutine cutsceneCoroutine;
    [SerializeField] float cutsceneSecondsLength;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapCutscene += BeginStartCutscene;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapCutscene -= BeginStartCutscene;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            BeginCutscene(CutsceneType.StartingCutscene);
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            EndCutscene();
        }
    }

    ///<summary>
    /// Begins the starting cutscene
    ///</summary>
    void BeginStartCutscene()
    {
        if(cutsceneCoroutine != null)
            StopCoroutine(cutsceneCoroutine);

        cutsceneCoroutine = StartCoroutine(CutsceneTimer(CutsceneType.StartingCutscene));
    }

    ///<summary>
    /// Coroutine to start and stop the cutscene
    ///</summary>
    IEnumerator CutsceneTimer(CutsceneType cutsceneType)
    {
        BeginCutscene(cutsceneType);
        yield return new WaitForSeconds(cutsceneSecondsLength);
        EndCutscene();
    }


    ///<summary>
    /// Begins a cutscene, the cutscene played is passed through with an enum typing
    ///</summary>
    void BeginCutscene(CutsceneType cutsceneType)
    {
        foreach(GameObject camPositions in cameraPositions)
        {
            camPositions.SetActive(true);
        }

        cutsceneCamera.enabled = true;
        cutsceneCanvas.enabled = true;

        playableDirector.Play();

        switch (cutsceneType)
        {
            case CutsceneType.StartingCutscene:
            case CutsceneType.FinalOrderCutscene:
            default: return;
        }
    }

    ///<summary>
    /// Calls method when cutscene is supposed to end
    ///</summary>
    void EndCutscene()
    {
        cutsceneCanvas.enabled = false;
        cutsceneCamera.enabled = false;

        foreach (GameObject camPositions in cameraPositions)
        {
            camPositions.SetActive(false);
        }

        GameManager.Instance.SetGameState(GameState.Begin);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoostPadManager : SingletonMonobehaviour<BoostPadManager>
{
    PlayerInstantiate playerInstantiate;
    [SerializeField] List<BoostPadUpdate> boostPads = new List<BoostPadUpdate>();
    [SerializeField] GameObject[] playersToKeepTrackOf;

    // Start is called before the first frame update
    void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;
        UpdateBoostPads();
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate independently
        for (int i = 0; i <= playersToKeepTrackOf.Length - 1; i++)
        {
            if (playersToKeepTrackOf[i] == null)
                continue;

            foreach(BoostPadUpdate boostPad in boostPads)
            {
                boostPad.UpdatePadRotation(playersToKeepTrackOf[i], i);
            }
        }
    }

    public void AddBoostPad(BoostPadUpdate boostPad)
    {
        boostPads.Add(boostPad);
    }

    public void UpdateBoostPads()
    {

        // Loops and adds player references
        playersToKeepTrackOf = new GameObject[4];

        for (int i = 0; i < playerInstantiate.PlayerInputs.Length; i++)
        {
            PlayerInput playerInput = playerInstantiate.PlayerInputs[i];

            if (playerInput != null)
            {
                playersToKeepTrackOf[i] = playerInput.gameObject.GetComponentInChildren<BallDriving>().gameObject;
            }
        }
    }

}

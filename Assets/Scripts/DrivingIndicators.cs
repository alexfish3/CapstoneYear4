using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrivingIndicators : MonoBehaviour
{
    [SerializeField] Camera iconCamera;
    PlayerInstantiate playerInstantiate;
    [SerializeField] PlayerInput thisPlayer;
    [SerializeField] GameObject thisPlayerGameobject;
    [SerializeField] List<GameObject> playersToKeepTrackOf = new List<GameObject>();
    [SerializeField] List<Transform> playerCameraTransforms = new List<Transform>();
    [SerializeField] GameObject[] playersRotationObjects;

    [SerializeField] float speed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        // The step size is equal to speed times frame time.
        float singleStep = speed * Time.deltaTime;

        // Rotate independently
        for (int i = 0; i <= playersToKeepTrackOf.Count - 1; i++)
        {
            playersRotationObjects[i].transform.rotation = Quaternion.Euler(new Vector3(
                playerCameraTransforms[i].eulerAngles.x,
                playerCameraTransforms[i].eulerAngles.y,
                playerCameraTransforms[i].eulerAngles.z));
        }

        // scale independently

    }

    public void UpdatePlayerReferencesForObjects()
    {
        // Sets all to false
        foreach(GameObject rotationObject in playersRotationObjects)
        {
            rotationObject.SetActive(false);
        }

        // Loops and adds player references
        int counter = 0;
        playersToKeepTrackOf.Clear();
        playerCameraTransforms.Clear();

        for(int i = 0; i < playerInstantiate.PlayerInputs.Length; i++)
        {
            PlayerInput playerInput = playerInstantiate.PlayerInputs[i];

            if (playerInput != null && playerInput != thisPlayer)
            {
                playersToKeepTrackOf.Add(playerInput.gameObject.GetComponentInChildren<BallDriving>().gameObject);

                playerCameraTransforms.Add(playerInput.gameObject.GetComponent<PlayerCameraResizer>().PlayerReferenceCamera.transform);

                playersRotationObjects[counter].SetActive(true);

                List<GameObject> needToSwitch = new List<GameObject>();
                needToSwitch.Add(playersRotationObjects[counter]);
                needToSwitch.Add(playersRotationObjects[counter].transform.GetChild(0).gameObject);

                PlayerCameraResizer.UpdatePlayerObjectLayer(needToSwitch, i, iconCamera);
                counter++;
            }
        }
    }
}

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

    [Header("Distance")]
    [SerializeField] float distanceScale = 15f;
    [SerializeField] Vector2 sizeValues;

    // Start is called before the first frame update
    void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            UpdatePlayerReferencesForObjects();
        }

        // Rotate independently
        for (int i = 0; i <= playersToKeepTrackOf.Count - 1; i++)
        {
            playersRotationObjects[i].transform.rotation = Quaternion.Euler(new Vector3(
                playerCameraTransforms[i].eulerAngles.x,
                playerCameraTransforms[i].eulerAngles.y,
                playerCameraTransforms[i].eulerAngles.z));
        }

        // scale independently
        for (int i = 0; i <= playersToKeepTrackOf.Count - 1; i++)
        {
            float distance = Vector3.Distance(thisPlayerGameobject.transform.position, playersToKeepTrackOf[i].transform.position);
            float sizeValue = Mathf.Clamp(distance / distanceScale, sizeValues.x, sizeValues.y);

            playersRotationObjects[i].transform.localScale = new Vector3(sizeValue, sizeValue, sizeValue);
        }
    }

    public void UpdatePlayerReferencesForObjects()
    {
        Debug.Log("Update Arrows");

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

                List<GameObject> needToSwitch = new List<GameObject>
                {
                    playersRotationObjects[counter],
                    playersRotationObjects[counter].transform.GetChild(0).gameObject
                };

                PlayerCameraResizer.UpdatePlayerObjectLayer(needToSwitch, i, iconCamera);
                counter++;
            }
        }
    }
}

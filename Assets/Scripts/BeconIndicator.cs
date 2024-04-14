using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BeconIndicator : MonoBehaviour
{
    PlayerInstantiate playerInstantiate;
    [SerializeField] GameObject thisBecon;
    [SerializeField] GameObject[] playersToKeepTrackOf;
    [SerializeField] Transform[] playerCameraTransforms;
    [SerializeField] GameObject[] beconRotationObjects;
    [SerializeField] Animator animator;

    [Header("Rotation Sprite Options")]
    [SerializeField] SpriteRenderer[] rotationSprites;
    [SerializeField] Sprite[] spriteOptions;

    [Header("Distance")]
    [SerializeField] float fadeDistance = 10f;
    [SerializeField] float fadeLength = 5f;
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
        // Rotate independently
        for (int i = 0; i <= playersToKeepTrackOf.Length - 1; i++)
        {
            if (playersToKeepTrackOf[i] == null)
                continue;

            beconRotationObjects[i].transform.rotation = Quaternion.Euler(new Vector3(
                playerCameraTransforms[i].eulerAngles.x,
                playerCameraTransforms[i].eulerAngles.y,
                playerCameraTransforms[i].eulerAngles.z));
        }

        // Scale independently
        for (int i = 0; i <= playersToKeepTrackOf.Length - 1; i++)
        {
            if (playersToKeepTrackOf[i] == null)
                continue;

            float distance = Vector3.Distance(thisBecon.transform.position, playersToKeepTrackOf[i].transform.position);

            float sizeValue = Mathf.Clamp(distance / distanceScale, sizeValues.x, sizeValues.y);

            beconRotationObjects[i].transform.localScale = new Vector3(sizeValue, sizeValue, sizeValue);

            Color cahceColor = rotationSprites[i].color;
            Color newColor = Color.white;

            // Fade Color
            if (distance <= fadeDistance)
            {
                float alpha = RangeMutations.Map_Linear(distance, fadeDistance - fadeLength, fadeDistance, 0f, 1);
                newColor = new Color(cahceColor.r, cahceColor.b, cahceColor.g, alpha);

                Debug.Log("Alpha is " + alpha);

            }
            // Normal
            else
            {
                newColor = Color.white;
            }
            rotationSprites[i].color = newColor;
            Debug.Log("New Alpha is " + newColor.a);
        }
    }

    public void InitalizeBeconIndicator(Constants.OrderValue orderValue)
    {
        Debug.Log("Initalize Becon Icons");

        foreach(SpriteRenderer renderer in rotationSprites)
        {
            switch (orderValue)
            {
                case Constants.OrderValue.Easy:
                    renderer.sprite = spriteOptions[0];
                    break;
                case Constants.OrderValue.Medium:
                    renderer.sprite = spriteOptions[1];
                    break;
                case Constants.OrderValue.Hard:
                    renderer.sprite = spriteOptions[2];
                    break;
                case Constants.OrderValue.Golden:
                    renderer.sprite = spriteOptions[3];
                    break;
            }
        }

        // Loops and adds player references
        playersToKeepTrackOf = new GameObject[4];
        playerCameraTransforms = new Transform[4];

        for (int i = 0; i < playerInstantiate.PlayerInputs.Length; i++)
        {
            PlayerInput playerInput = playerInstantiate.PlayerInputs[i];

            if (playerInput != null)
            {
                playersToKeepTrackOf[i] = playerInput.gameObject.GetComponentInChildren<BallDriving>().gameObject;
                playerCameraTransforms[i] = playerInput.gameObject.GetComponent<PlayerCameraResizer>().PlayerReferenceCamera.transform;
                beconRotationObjects[i].SetActive(true);
            }
        }

        // plays the spawn animation for the becon
        animator.SetTrigger(HashReference._spawnTrigger);
    }

    public void RemoveBeconIndicator()
    {
        Debug.Log("Delete Becon Icons");

        // Sets all to false
        foreach (GameObject beconSprite in beconRotationObjects)
        {
            beconSprite.SetActive(false);
        }

        // plays the reset animation for the becon
        animator.SetTrigger(HashReference._resetTrigger);
    }
}

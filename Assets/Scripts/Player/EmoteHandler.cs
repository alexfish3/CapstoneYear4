using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is for displaying emotes on the player's driving canvas.
/// </summary>
public class EmoteHandler : MonoBehaviour
{
    [Tooltip("GO on the player prefab with the InputManager component.")]
    [SerializeField] private InputManager input;

    [Tooltip("Image on the canvas where the emotes will pop up.")]
    [SerializeField] private Image emoteImg;

    [Tooltip("Array of sprite emotes. [0]: Up, [1]: Right, [2]: Down, [3]: Left.")]
    [SerializeField] private Sprite[] emoteSprites;
    private Sprite currEmote;
    
    [Tooltip("How long the emote stays on the screen for.")]
    [SerializeField] private float emoteLifetime;
    
    private IEnumerator emoteShowRoutine;
    // Start is called before the first frame update
    void Start()
    {
        ResetEmote();
    }

    private void OnEnable()
    {
        input.DPadEvent += Emote;
    }
    private void OnDisable()
    {
        input.DPadEvent -= Emote;
    }

    /// <summary>
    /// Resets the emote image to its original state.
    /// </summary>
    private void ResetEmote()
    {
        emoteImg.gameObject.SetActive(false);
        emoteImg.sprite = null;
        currEmote = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dpad"></param>
    public void Emote(Vector2 dpad)
    {
        Sprite emote = null;
        switch(dpad.x)
        {
            case 1:
                emote = emoteSprites[1];
                break;
            case -1:
                emote = emoteSprites[3];
                break;
            default:
                switch(dpad.y)
                {
                    case 1:
                        emote = emoteSprites[0];
                        break;
                    case -1:
                        emote = emoteSprites[2];
                        break;
                    default:
                        break;
                }
                break;
        }
        if(emote != null)
        {
            StopEmoteShow();
            currEmote = emote;
            StartEmoteShow();
        }
    }

    private void StartEmoteShow()
    {
        if(emoteShowRoutine == null)
        {
            emoteShowRoutine = ShowEmote();
            StartCoroutine(emoteShowRoutine);
        }
    }

    private void StopEmoteShow()
    {
        if(emoteShowRoutine != null)
        {
            StopCoroutine(emoteShowRoutine);
            emoteShowRoutine = null;
            ResetEmote();
        }
    }

    /// <summary>
    /// Shows the emote for a specified amout of time and then disable it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowEmote()
    {
        emoteImg.sprite = currEmote;
        emoteImg.gameObject.SetActive(true);
        yield return new WaitForSeconds(emoteLifetime);
        emoteImg.sprite = null;
        ResetEmote();
    }
}

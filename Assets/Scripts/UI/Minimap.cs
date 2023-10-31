using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class sets up a render texture from the minimap cam and uses it for the minimap on the player
/// /// </summary>
public class Minimap : MonoBehaviour
{
    [Tooltip("The minimap camera. This should be labeled as such on the player prefab")]
    [SerializeField] Camera minimapCamera;

    [Tooltip("This is generated on run-time, keep this empty in editor")]
    [SerializeField] RenderTexture minimap;

    [Tooltip("This is the raw image the render texture will draw to, this is in the ui canvas on the player")]
    [SerializeField] RawImage minimapPos;

    // Start is called before the first frame update
    void Start()
    {
        minimap = new RenderTexture(256,256, 8);
        minimapCamera.targetTexture = minimap;
        minimapPos.texture = minimap;
    }
}

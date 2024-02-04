using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Camera))]
public class HeatmapCamera : MonoBehaviour
{
    [Tooltip("Position for the main map heat map")]
    [SerializeField] private Transform mainMapPos;
    [Tooltip("Position for the final map heat map")]
    [SerializeField] private Transform finalMapPos;
    private Camera viewport;
    private void Awake()
    {
        viewport = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnSwapGoldenCutscene += TakePicture;
        GameManager.Instance.OnSwapResults += TakeFinalPicture;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapGoldenCutscene -= TakePicture;
        GameManager.Instance.OnSwapResults -= TakeFinalPicture;
    }

    private void TakePicture()
    {
        if (!QAManager.Instance.GenerateHeatmap)
            return;
        this.transform.position = mainMapPos.position;
        this.transform.rotation = mainMapPos.rotation;

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        viewport.targetTexture = rt;

        Texture2D image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        viewport.Render();

        RenderTexture.active = rt;
        image.ReadPixels(new Rect(0, 0, viewport.targetTexture.width, viewport.targetTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = null;
        viewport.targetTexture = null;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        int screenshotNum = PlayerPrefs.GetInt("screenshot_main", 0);
        File.WriteAllBytes(Application.streamingAssetsPath + "/HeatMaps/main_" + screenshotNum.ToString() + ".png", bytes);
        PlayerPrefs.SetInt("screenshot_main", screenshotNum+1);
    }

    private void TakeFinalPicture()
    {
        if (!QAManager.Instance.GenerateHeatmap)
            return;

        this.transform.position = finalMapPos.position;
        this.transform.rotation = finalMapPos.rotation;

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        viewport.targetTexture = rt;

        Texture2D image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        viewport.Render();

        RenderTexture.active = rt;
        image.ReadPixels(new Rect(0, 0, viewport.targetTexture.width, viewport.targetTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = null;
        viewport.targetTexture = null;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        int screenshotNum = PlayerPrefs.GetInt("screenshot_final", 0);
        File.WriteAllBytes(Application.streamingAssetsPath + "/HeatMaps/final_" + screenshotNum.ToString() + ".png", bytes);
        PlayerPrefs.SetInt("screenshot_final", screenshotNum+1);
    }
}

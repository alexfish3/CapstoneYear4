using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class QAManager : SingletonMonobehaviour<QAManager>
{
    [Tooltip("Toggle for recording data in the text document.")]
    [SerializeField] private bool recordData = true;

    private List<QAHandler> handlers = new List<QAHandler>();

    private string outputFile = "/QA/GameHistory.txt";
    private void Start()
    {
        GameManager.Instance.OnSwapResults += SendData;
        GameManager.Instance.OnSwapBegin += ResetHandlers;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapResults -= SendData;
        GameManager.Instance.OnSwapBegin -= ResetHandlers;
    }

    public void AddHandler(QAHandler inHandler)
    {
        if (!handlers.Contains(inHandler))
        {
            handlers.Add(inHandler);
        }
    }

    public void RemoveHandler(QAHandler outHandler)
    {
        if(handlers.Contains(outHandler))
        {
            handlers.Remove(outHandler);
        }
    }

    private void SendData()
    {
        if (!recordData) { return; }
        string path = Application.dataPath + outputFile;
        Debug.Log($"Path: {path}");
        try
        {
            using(StreamWriter writer = new StreamWriter(path, true))
            {
                int currentGame = PlayerPrefs.GetInt("qaCount", 0);
                writer.WriteLine($"~Start Game {currentGame}~\n");
                foreach(QAHandler handler in handlers)
                {
                    writer.WriteLine(handler.GetData());
                    writer.WriteLine("--\n");
                }
                writer.Write($"Final Order Value: ${OrderManager.Instance.FinalOrderValue}\n");
                writer.WriteLine($"~End Game {currentGame}~");
                currentGame++;
                PlayerPrefs.SetInt("qaCount", currentGame);
                PlayerPrefs.Save();
            }

        }
        catch
        {
            Debug.LogError("Couldn't write to file.");
        }
    }

    private void ResetHandlers()
    {
        foreach(QAHandler handler in handlers)
        {
            handler.ResetQA();
        }
    }
}

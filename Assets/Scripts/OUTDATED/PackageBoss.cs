using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageBoss : MonoBehaviour
{
    public static PackageBoss Instance;
    private List<Package> packages = new List<Package>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        foreach(Transform child in transform)
        {
            packages.Add(child.GetComponent<Package>());
        }
    }
    private void Update()
    {
        if(packages.Count <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }
}

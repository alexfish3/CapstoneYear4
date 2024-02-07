using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianManager : MonoBehaviour
{
    [Header("Pedestrian Setup")]
    [Tooltip("How many civilians to create")]
    [SerializeField] private int pedestrianCount = 10;
    [Tooltip("Civilian prefab reference")]
    [SerializeField] private GameObject civilianPrefab;

    [Header("Waypoints")]
    [Tooltip("List of waypoints, which should all rest a slight distance above ground level")]
    [SerializeField] private Transform[] waypoints;
    [Tooltip("How many waypoints each *pedestrian* should have in their net")]
    [SerializeField] private int pedestrianWaypointCount = 4;

    private GameObject[] pedestrians;

    private void Start()
    {
        pedestrians = new GameObject[pedestrianCount];

        for (int i = 0; i < pedestrianCount; i++)
        {
            
            

            Transform[] chosenPoints = new Transform[pedestrianWaypointCount];

            ShuffleWaypoints();
            for (int j = 0; j < pedestrianWaypointCount; j++)
            {
                chosenPoints[j] = waypoints[j];
            }

            GameObject pedestrian = Instantiate(civilianPrefab, chosenPoints[0].position, Quaternion.identity);
            CivilianAgent pedestrianAgent = pedestrian.GetComponent<CivilianAgent>();

            //pedestrian.transform.position = chosenPoints[0].position;
            pedestrianAgent.Points = chosenPoints;
            pedestrianAgent.BeginPathing();
        }
    }

    /// <summary>
    /// Implements the Fisher-Yates shuffle to shuffle the list of waypoints
    /// </summary>
    private void ShuffleWaypoints()
    {
        for (int n = waypoints.Length - 1; n > 0; n--)
        {
            int k = Random.Range(0, n + 1);

            Transform temp = waypoints[n];
            waypoints[n] = waypoints[k];
            waypoints[k] = temp;
        }
    }
}

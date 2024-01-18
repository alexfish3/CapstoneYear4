using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    [SerializeField] RawImage compassImage;
    [SerializeField] Transform compassCalc;
    [SerializeField] Transform player;

    [SerializeField] OrbitalCamera orbitalCamera;

    [SerializeField] Camera playerCam;

    [Tooltip("The y position of the ui on the compass")]
    [SerializeField] float iconHeight = -70;

    //[Tooltip("The distance away from the object before it fades on the compass ui. In meters")]
    //[SerializeField] float fadeDistance = 100f;

    [SerializeField] GameObject iconPrefab;
    [SerializeField] List<CompassIconUI> compassUIObjects = new List<CompassIconUI>();
    public List<CompassIconUI> CompassUIObjects { get { return compassUIObjects; } }

    [SerializeField] List<CompassMarker> compassMarkerObjects = new List<CompassMarker>();
    public List<CompassMarker> CompassMarkerObjects { get { return compassMarkerObjects; } }

    [SerializeField] float compassUnit;

    private void OnEnable()
    {
        Debug.Log("RESET COMPASS");
        GameManager.Instance.OnSwapMenu += ResetCompass;
    }

    private void OnDisable()
    {
        Debug.Log("DISABLE RESET COMPASS");
        GameManager.Instance.OnSwapMenu -= ResetCompass;
    }

    private void Start()
    {
        // Calculates compass unit based on the size of the compass image ui size
        compassUnit = compassImage.rectTransform.rect.width / 360f;
    }

    private void Update()
    {
        // Updates the uv rect of the compass image, to scroll based on player rotation
        compassImage.uvRect = new Rect((player.localEulerAngles.y + orbitalCamera.smoothAxis) / 360f, 0f, 1f, 1f);

        // Loops for all markers on player and updates their position on the compass ui
        foreach (CompassIconUI marker in compassUIObjects)
        {
            // Updates the position on the compass
            //marker.imageRect.rectTransform.anchoredPosition = GetPosOnCompass(marker.objectReference);
            marker.imageRect.rectTransform.anchoredPosition = GetPosOnCompass(marker.objectReference);

            // Calculates the distance of the player to the object
            marker.distance = CalculateDistance(marker.objectReference);
            marker.SetDistanceText();

            // Determines if it is time to fade in or out marker, based on fade distance
            if (marker.distance < Constants.DISTANCE_TO_FADE && marker.Faded == false)
            {
                marker.FadeMarkerOut();
            }
            else if (marker.distance > Constants.DISTANCE_TO_FADE && marker.Faded == true)
            {
                marker.FadeMarkerIn();
            }
        }

        var sortedListDescending = compassUIObjects.OrderByDescending(ui => ui.distance).ToList();

        // Loop through the sorted list and organize the children
        for (int i = 0; i < sortedListDescending.Count; i++)
        {
            // Assuming each CompassIconUI has a transform property
            sortedListDescending[i].transform.SetSiblingIndex(i);
        }

    }

    ///<summary>
    /// Adds a compass marker to the player's compass
    ///</summary>
    public void AddCompassMarker(CompassMarker marker)
    {
        // Creates new object
        GameObject newMarker = Instantiate(iconPrefab, compassImage.transform);
        CompassIconUI compassIconUI = newMarker.GetComponent<CompassIconUI>();

        compassIconUI.SetCompassIconSprite(marker.icon);
        compassIconUI.objectReference = marker;

        // Adds to list
        compassMarkerObjects.Add(marker);
        compassUIObjects.Add(compassIconUI);
    }

    ///<summary>
    /// Removes a compass marker from the player's compass and deletes the icon
    ///</summary>
    public void RemoveCompassMarker(CompassMarker marker)
    {
        int indexOfObject = compassMarkerObjects.IndexOf(marker);

        if(indexOfObject > -1)
        {
            Debug.Log(indexOfObject);

            // Cahces the ui icon to remove
            GameObject objectToRemove = compassUIObjects[indexOfObject].gameObject;

            // Removes from lists
            compassMarkerObjects.RemoveAt(indexOfObject);
            compassUIObjects.RemoveAt(indexOfObject);

            // Deletes ui icon
            Destroy(objectToRemove);
        }
    }

    ///<summary>
    /// Swaps the compass icon for one that displays the marker being carried by a player
    /// isCarried indicates a scooter icon
    /// !isCarried indicates a floor icon
    ///</summary>
    public void ChangeCompassMarkerIcon(CompassMarker marker, bool isCarried)
    {
        Debug.Log("Change Icon");
    }

    ///<summary>
    /// Gets pos of compass marker and returns a vector2 transform for the compas icons
    ///</summary>
    Vector2 GetPosOnCompass(CompassMarker marker)
    {
        // Calculate vector from player to object
        Vector3 playerToObjectVector = marker.transform.position - compassCalc.transform.position;

        // Calculate player's forward vector based on the rotation
        Vector3 playerForwardVector = compassCalc.transform.rotation * Vector3.forward;

        // Calculate the dot product
        float dotProduct = Vector3.Dot(playerToObjectVector.normalized, playerForwardVector.normalized);

        // Calculate the angle in radians
        float angleRadians = Mathf.Acos(dotProduct);

        // Convert the angle to degrees, and adjust for negative values when turning counterclockwise (west)
        float angleDegrees = Mathf.Rad2Deg * angleRadians;

        // Adjust the angle to be negative when turning counterclockwise
        angleDegrees = (Vector3.Cross(playerForwardVector, playerToObjectVector).y < 0) ? -angleDegrees : angleDegrees;

        return new Vector2((angleDegrees - orbitalCamera.smoothAxis) * compassUnit, iconHeight);
    }

    ///<summary>
    /// Returns the distance of the marker to the player
    ///</summary>
    int CalculateDistance(CompassMarker marker)
    {
        return (int) Vector3.Distance(marker.transform.position, compassCalc.transform.position);
    }

    ///<summary>
    /// Resets compass by removing any references
    ///</summary>
    public void ResetCompass()
    {
        Debug.Log("Reset Compass");
        foreach(CompassIconUI icon in compassUIObjects)
        {
            Destroy(icon.gameObject);
        }
        compassUIObjects.Clear();

        compassMarkerObjects.Clear();
    }
}

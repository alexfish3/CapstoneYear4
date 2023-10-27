using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    [SerializeField] RawImage compassImage;
    [SerializeField] Transform player;

    [SerializeField] GameObject iconPrefab;
    [SerializeField] List<GameObject> compassMarkerObjects = new List<GameObject>();

    float compassUnit;

    private void Start()
    {
        // Calculates compass unit based on the size of the compass image ui size
        compassUnit = compassImage.rectTransform.rect.width / 360f;
    }

    private void Update()
    {
        // Updates the uv rect of the compass image, to scroll based on player rotation
        compassImage.uvRect = new Rect(player.localEulerAngles.y / 360f, 0f, 1f, 1f);

        // Loops for all markers on player and updates their position on the compass ui
        foreach (GameObject marker in compassMarkerObjects)
        {
            // TO-DO: Add calculation to render close markers over far markers. Right now just renders new over old

            CompassIconUI compassIconUI = marker.GetComponent<CompassIconUI>();
            compassIconUI.imageRect.rectTransform.anchoredPosition = GetPosOnCompass(compassIconUI.objectReference);
        }
    }

    ///<summary>
    /// Adds a compass marker to the player's compass
    ///</summary>
    public void AddCompassMarker(CompassMarker marker)
    {
        // Creates new object
        GameObject newMarker = Instantiate(iconPrefab, compassImage.transform);
        newMarker.GetComponent<Image>().sprite = marker.icon;
        newMarker.GetComponent<CompassIconUI>().objectReference = marker;

        // Adds to list
        compassMarkerObjects.Add(newMarker);
    }

    ///<summary>
    /// Gets pos of compass marker and returns a vector2 transform for the compas icons
    ///</summary>
    Vector2 GetPosOnCompass(CompassMarker marker)
    {
        // Calculate vector from player to object
        Vector3 playerToObjectVector = marker.transform.position - player.transform.position;

        // Calculate player's forward vector based on the rotation
        Vector3 playerForwardVector = player.transform.rotation * Vector3.forward;

        // Calculate the dot product
        float dotProduct = Vector3.Dot(playerToObjectVector.normalized, playerForwardVector.normalized);

        // Calculate the angle in radians
        float angleRadians = Mathf.Acos(dotProduct);

        // Convert the angle to degrees, and adjust for negative values when turning counterclockwise (west)
        float angleDegrees = Mathf.Rad2Deg * angleRadians;

        // Adjust the angle to be negative when turning counterclockwise
        angleDegrees = (Vector3.Cross(playerForwardVector, playerToObjectVector).y < 0) ? -angleDegrees : angleDegrees;

        return new Vector2(angleDegrees * compassUnit, 0);
    }
}

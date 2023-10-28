using TMPro;
using UnityEngine;
using UnityEngine.UI;

///<summary>
/// To be put on the compass icon prefab, holds info to the real world object
///</summary>
public class CompassIconUI : MonoBehaviour
{
    public Image imageRect;
    public CompassMarker objectReference;
    public TMP_Text distanceText;

    ///<summary>
    /// Updates the distance text on the icon, appends the m on the int distance. Josh's Idea
    ///</summary>
    public void SetDistanceText(int distance)
    {
        if(Constants.DISTANCE_TYPE == Constants.DistanceType.Meters)
        {
            distanceText.text = distance.ToString() + "m";
        }
        else if (Constants.DISTANCE_TYPE == Constants.DistanceType.Feet)
        {
            distanceText.text = distance.ToString() + "ft";
        }
    }


}

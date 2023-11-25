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
    public Animator animator;
    public int distance;

    [Tooltip("Keeps track if the value has been faded on the ui or not")]
    [SerializeField] bool faded = false;
    public bool Faded { get { return faded; }}

    ///<summary>
    /// triggers the ui fade out
    ///</summary>
    public void FadeMarkerOut()
    {
        faded = true;
        animator.SetTrigger("FadeOut");
    }

    ///<summary>
    /// triggers the ui fade in
    ///</summary>
    public void FadeMarkerIn()
    {
        faded = false;
        animator.SetTrigger("FadeIn");
    }

    ///<summary>
    /// Updates the distance text on the icon, appends the m on the int distance. Josh's Idea
    ///</summary>
    public void SetDistanceText()
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

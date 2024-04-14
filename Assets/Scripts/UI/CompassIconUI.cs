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
    public Image mainIcon, leftChildIcon, rightChildIcon;
    public TMP_Text distanceText, distanceTextChildLeft, distanceTextChildRight;
    public Animator animator, animatorChildLeft, animatorChildRight;
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
        animator.SetTrigger(HashReference._fadeOutTrigger);
        animatorChildLeft.SetTrigger(HashReference._fadeOutTrigger);
        animatorChildRight.SetTrigger(HashReference._fadeOutTrigger);
    }

    ///<summary>
    /// triggers the ui fade in
    ///</summary>
    public void FadeMarkerIn()
    {
        faded = false;
        animator.SetTrigger(HashReference._fadeInTrigger);
        animatorChildLeft.SetTrigger(HashReference._fadeInTrigger);
        animatorChildRight.SetTrigger(HashReference._fadeInTrigger);
    }

    public void SetCompassIconSprite(Sprite sprite)
    {
        mainIcon.sprite = sprite;
        leftChildIcon.sprite = sprite;
        rightChildIcon.sprite = sprite;
    }

    ///<summary>
    /// Updates the distance text on the icon, appends the m on the int distance. Josh's Idea
    ///</summary>
    public void SetDistanceText()
    {
        distanceText.text = distance.ToString() + "ft";
        distanceTextChildLeft.text = distance.ToString() + "ft";
        distanceTextChildRight.text = distance.ToString() + "ft";
    }


}

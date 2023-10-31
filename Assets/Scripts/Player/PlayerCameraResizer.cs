using UnityEngine;

/// <summary>
/// This class resizes the rect of all cameras in array, based on a reference cam. 
/// This is needed due to the player input script only allowing one camera to be resized on instantiation. 
/// This acts to allow all cameras needed to be resized, after the main is resized.
/// /// </summary>
public class PlayerCameraResizer : MonoBehaviour
{
    [Tooltip("This is the camera in which if changed, other cameras will also change to match")]
    [SerializeField] Camera referenceCam;

    [Tooltip("This is the camera array in which all will resize to the main upon main being changed")]
    [SerializeField] Camera[] camerasToFollow;

    [Tooltip("This is a vector4 value indicating the default rect of a camera. the four values are xPos, yPos, Width and Height")]
    [SerializeField] Vector4 viewPortRectDefault;

    bool initalized = false;

    // Update is called once per frame
    void Update()
    {
        // Returns if updated already
        if (initalized)
            return;

        // If it does not equal default, then player is initalized and the other cameras should follow.
        if(referenceCam.rect.x != viewPortRectDefault.x || referenceCam.rect.y != viewPortRectDefault.y 
            || referenceCam.rect.width != viewPortRectDefault.z || referenceCam.rect.height != viewPortRectDefault.w)
        {
            Debug.Log("Camera Resizing Needed! Will resize all cameras on stack");

            // Loops through each cam
            foreach(Camera cam in camerasToFollow)
            {
                // Updates to be equal to reference cam
                cam.rect = new Rect(referenceCam.rect.x, referenceCam.rect.y, referenceCam.rect.width, referenceCam.rect.height);
            }

            initalized = true;
        }
    }
}

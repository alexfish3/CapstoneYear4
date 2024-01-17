using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    [Tooltip("This reference is to the camera outputing to the render textures")]
    [SerializeField] Camera playerRenderCamera;
    public Camera PlayerRenderCamera { get { return playerRenderCamera; } }

    [Header("Cinemachine Info")]
    [SerializeField] GameObject[] virtualCameras;

    [Header("UI References")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera phaseCamera;
    [SerializeField] Camera playerCamera;
    [SerializeField] Camera menuUICamera;
    [SerializeField] Camera drivingUICamera;
    [SerializeField] GameObject customizationSelector;

    bool initalized = false;
    [SerializeField] bool enableCameraSwap = true;

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

    ///<summary>
    /// Updates the cameras to render certain layers based on the player
    ///</summary>
    public void UpdateVirtualCameras(int nextFillSlot)
    {
        int cameraLayer = 0;

        // Gets camera layer based on player
        if (nextFillSlot == 1)
            cameraLayer = 17;
        else if (nextFillSlot == 2)
            cameraLayer = 18;
        else if (nextFillSlot == 3)
            cameraLayer = 19;
        else if (nextFillSlot == 4)
            cameraLayer = 20;

        // Loops and sets all virtual cams to layer
        foreach(GameObject virtualCam in virtualCameras)
            virtualCam.layer = cameraLayer;

        // Add via bitwise to include the camera layer
        referenceCam.cullingMask |= (1 << cameraLayer);
    }

    ///<summary>
    /// Toggles to swap canvas on player. True sets menu canvas on
    ///</summary>
    public void SwapCanvas(bool menuCanvasOn)
    {
        if (menuCanvasOn)
        {
            menuUICamera.enabled = true;
            drivingUICamera.enabled = false;
            //drivingCanvas.SetActive(false);
        }
        else 
        {
            menuUICamera.enabled = false;
            drivingUICamera.enabled = true;
            //drivingCanvas.SetActive(true);
        }
    }

    ///<summary>
    /// Toggles to swap player camera rendering to and from phase camera
    ///</summary>
    public void SwapCameraRendering(bool mainCameraOn)
    {
        if (!enableCameraSwap)
        {
            return;
        }
        if (mainCameraOn)
        {
            mainCamera.enabled = true;
            phaseCamera.enabled = false;
        }
        else
        {
            mainCamera.enabled = false;
            phaseCamera.enabled = true;
        }
    }

    ///<summary>
    /// Relocates the character customization ui to the left if player is even number (2 or 4)
    ///</summary>
    public void RelocateCustomizationMenu(int playerNumber)
    {
        // Even
        if(playerNumber % 2 == 0)
        {
            customizationSelector.transform.position = new Vector3(-customizationSelector.transform.position.x, customizationSelector.transform.position.y,
                customizationSelector.transform.position.z);
        }
    }

    /// <summary>
    /// Reparents the ui camera on the two cameras that can use it in a stack. (true is mainCamera, false is playerCamera)
    /// </summary>
    /// <param name="direction"></param>
    public void ReparentMenuCameraStack(bool posOfMenuCamera)
    {
        // Reparent to main camera
        if(posOfMenuCamera)
        {
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(menuUICamera);
            playerCamera.GetUniversalAdditionalCameraData().cameraStack.Remove(menuUICamera);
        }
        // Reparent to player camera
        else
        {
            mainCamera.GetUniversalAdditionalCameraData().cameraStack.Remove(menuUICamera);
            playerCamera.GetUniversalAdditionalCameraData().cameraStack.Add(menuUICamera);
        }
    }
}

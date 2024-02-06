using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{
    [SerializeField] InputManager inputManager;

    [Header("Horizontal Movement")]
    [SerializeField] CinemachineVirtualCamera virtualCameraMain;
    [SerializeField] CinemachineVirtualCamera virtualCameraIcon;
    [SerializeField] CinemachineOrbitalTransposer mainOrb;
    [SerializeField] CinemachineOrbitalTransposer iconOrb;
    [SerializeField] float maxXAngle = 180;
    [SerializeField] float smoothSpeedValue = 0.1f;
    [SerializeField] float realXAxis;
    public float smoothXAxis;

    [Header("Vertical Movement")]
    [SerializeField] GameObject CameraFocus;
    [SerializeField] Vector2 yAngleMinMax;
    [SerializeField] float realYAxis;
    public float smoothYAxis;

    // Start is called before the first frame update
    void Start()
    {
        mainOrb = virtualCameraMain.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        iconOrb = virtualCameraIcon.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Custom joystick camera aim
        if (!inputManager.RightStickValue)
        {
            realXAxis = RangeMutations.Map_Linear(inputManager.RightStickXValue, -1, 1, -maxXAngle, maxXAngle);
            realYAxis = RangeMutations.Map_Linear(inputManager.RightStickYValue, -1, 1, yAngleMinMax.x, yAngleMinMax.y);

            smoothXAxis = Mathf.Lerp(smoothXAxis, realXAxis, smoothSpeedValue);
            smoothYAxis = Mathf.Lerp(smoothYAxis, realYAxis, smoothSpeedValue);

            mainOrb.m_XAxis.Value = smoothXAxis;
            iconOrb.m_XAxis.Value = smoothXAxis;

            CameraFocus.transform.localPosition = new Vector3(CameraFocus.transform.localPosition.x, smoothYAxis, CameraFocus.transform.localPosition.z);
        }
        // Static look behind
        else
        {
            mainOrb.m_XAxis.Value = -180;
            iconOrb.m_XAxis.Value = 0;

            CameraFocus.transform.localPosition = new Vector3(CameraFocus.transform.localPosition.x, smoothYAxis, CameraFocus.transform.localPosition.z);
        }
    }
}

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{
    [SerializeField] InputManager inputManager;

    [Header("Horizontal Movement")]
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] CinemachineOrbitalTransposer orb;
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
        orb = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        realXAxis = RangeMutations.Map_Linear(inputManager.RightStickXValue, -1, 1, -maxXAngle, maxXAngle);
        realYAxis = RangeMutations.Map_Linear(inputManager.RightStickYValue, -1, 1, yAngleMinMax.x, yAngleMinMax.y);

        smoothXAxis = Mathf.Lerp(smoothXAxis, realXAxis, smoothSpeedValue);
        smoothYAxis = Mathf.Lerp(smoothYAxis, realYAxis, smoothSpeedValue);

        orb.m_XAxis.Value = smoothXAxis;
        CameraFocus.transform.position = new Vector3(CameraFocus.transform.position.x, smoothYAxis, CameraFocus.transform.position.z);
    }
}

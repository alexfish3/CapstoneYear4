using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{
    [SerializeField] InputManager inputManager;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] CinemachineOrbitalTransposer orb;
    [SerializeField] float maxAngle = 180;
    [SerializeField] float smoothSpeedValue = 0.1f;
    [SerializeField] float realAxis;
    public float smoothAxis;

    // Start is called before the first frame update
    void Start()
    {
        orb = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        realAxis = RangeMutations.Map_Linear(inputManager.RightStickValue, -1, 1, -maxAngle, maxAngle);

        smoothAxis = Mathf.Lerp(smoothAxis, realAxis, smoothSpeedValue);

        orb.m_XAxis.Value = smoothAxis;
    }
}

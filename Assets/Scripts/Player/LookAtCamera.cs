using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [Tooltip("Target camera")]
    [SerializeField] private Camera viewport;

    private void LateUpdate()
    {
        transform.LookAt(viewport.transform, Vector3.up);
    }
}

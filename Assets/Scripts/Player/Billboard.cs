using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Tooltip("Camera you want the object to face")]
    [SerializeField] private Camera viewport;

    private void Update()
    {
        transform.LookAt(viewport.transform.position, Vector3.up);
    }
}

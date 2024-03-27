using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPadUpdate : MonoBehaviour
{
    [SerializeField] bool enableUpdate;
    [SerializeField] Material referenceArrowMaterial;
    [SerializeField] MeshRenderer[] meshRenderers;

    [SerializeField] bool added = false;

    [SerializeField] float angle;

    private void Start()
    {
        foreach(var renderer in meshRenderers)
        {
            renderer.material = new Material(referenceArrowMaterial);
        }
    }

    private void Update()
    {
        if (enableUpdate == false)
            return;

        if (added == false)
        {
            added = true;
            BoostPadManager.Instance.AddBoostPad(this);
        }
    }

    public void UpdatePadRotation(GameObject playerPassin, int playerPosition)
    {
        var xDistance = playerPassin.transform.position.x - this.transform.position.x;
        var zDistance = playerPassin.transform.position.z - this.transform.position.z;

        angle = (Mathf.Atan2(zDistance, xDistance) * Mathf.Rad2Deg) + gameObject.transform.rotation.y;

        meshRenderers[playerPosition].material.SetFloat("_RotateAxis", angle);

    }

}

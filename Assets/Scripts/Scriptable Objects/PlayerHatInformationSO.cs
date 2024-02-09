using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Hat Information", menuName = "Customization/Player Hat Information", order = 2)]
public class PlayerHatInformationSO : ScriptableObject
{
    [Header("Hat Information")]
    public bool displayHat;
    public Mesh hatMesh;
    public Material hatMaterial;
    public Sprite hatIcon;

    [Header("Transform Controls")]
    public Vector3 hatPosition;
    public Vector3 hatScale;
}

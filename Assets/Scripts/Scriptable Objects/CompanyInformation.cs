using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Company Information", menuName = "Customization/Company Information", order = 3)]
public class CompanyInformation : ScriptableObject
{
    [Header("Company Information")]
    public Company companyName;
    public Color companyColor;
    public Material scooterColorMaterial;
    public Material scooterDecalMaterial;

    [Header("Player Information")]
    public RenderTexture playerTexture;
}

public enum Company
{
    DOOR_DEATH,
    GHOSTMATES,
    SKIP_THE_SCARE,
    UBER_CREEPS
}

using UnityEngine;

[CreateAssetMenu(fileName = "Character Color", menuName = "Character Customization/Character Color")]
public class CharacterColorSO : ScriptableObject
{
    [ColorUsageAttribute(true, true)]
    public Color mainColor;

    public Color shadowColor;
}

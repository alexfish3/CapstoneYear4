using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Object", menuName = "AudioObject", order = 1)]
public class AudioObject : ScriptableObject
{
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
}

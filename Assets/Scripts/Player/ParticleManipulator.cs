using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManipulator : MonoBehaviour
{
    private ParticleSystem thisPart;

    private void Awake()
    {
        thisPart = GetComponent<ParticleSystem>();
    }

    public Color StartColor
    { 
        set
        {
            var main = thisPart.main;
            main.startColor = value;
        }
    }
}

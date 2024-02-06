using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    [Tooltip("How much speed to grant")]
    [SerializeField] private float speedBoostAmount = 160;
    public float SpeedBoostAmount => speedBoostAmount;
}

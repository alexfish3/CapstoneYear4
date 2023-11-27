using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class OrderInfo : ScriptableObject
{
    [SerializeField] protected Constants.OrderValue value;
    [SerializeField] protected Transform pickup;
    [SerializeField] protected Transform dropoff;
}

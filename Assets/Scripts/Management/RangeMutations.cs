using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// A helper class for mapping ranges of values to different ranges, which is commonly needed for the BallDriving script
/// </summary>
public static class RangeMutations
{
    public static float Map_SpeedToSteering(float inp, float maxSpeed)
    {
        float processedValue = inp / maxSpeed;
        processedValue = (-Mathf.Pow(((1.4f * processedValue) - 0.65f), 2.0f)) + processedValue + 0.4f; //Applies a "simple" parabolic function. I use way too many parentheses

        return processedValue;
    }
}

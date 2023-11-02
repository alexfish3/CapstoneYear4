using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// A helper class for mapping ranges of values to different ranges, which is commonly needed for the BallDriving script
/// </summary>
public static class RangeMutations
{
    /// <summary>
    /// Used for scaling steering based on speed. Takes values in a range from 0 to accelerationPower, then outputs values in a range from 0 to 1.
    /// Uses a parabolic function that basically makes it so that 0 maps to 0, 0.7 maps to 1, and 1 maps to about 0.85, all on a nice quadratic curve.
    /// Specifically uses the equation -(1.4x - 0.65)^2 + x + 0.408
    /// If anyone can find a better curve, let me know; this one doesn't quite hit 1 (highest it gets is 0.9998), and has a slight deadzone around 0
    /// </summary>
    /// <param name="inp">the input speed; aka the currentForce variable</param>
    /// <param name="maxSpeed">the max speed; aka the accelerationPower variable</param>
    /// <returns></returns>
    public static float Map_SpeedToSteering(float inp, float maxSpeed)
    {
        float processedValue = inp / maxSpeed;
        processedValue = (-Mathf.Pow(((1.4f * processedValue) - 0.65f), 2.0f)) + processedValue + 0.408f; //Applies a "simple" parabolic function. I use way too many parentheses

        return processedValue;
    }
}

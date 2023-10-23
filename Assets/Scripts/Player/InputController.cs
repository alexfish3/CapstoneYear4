using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

/// <summary>
/// Gets input from the controller and stores it in accessible variables.
/// Some of these are accessed constantly, some push events. Depends on the purpose of the control.
/// </summary>
public class InputManager : MonoBehaviour
{
    private float leftStickValue; //a value from -1 to 1, which represents the horizontal position of the left stick
    public float LeftStickValue { get { return leftStickValue; } }
    private float rightTriggerValue; //a value from 0 to 1, which represents the pull of the right trigger
    public float RightTriggerValue { get { return rightTriggerValue; } }
    private float leftTriggerValue; //a value from 0 to 1, which represents the pull of the left trigger
    public float LeftTriggerValue { get { return leftTriggerValue; } }

    /// <summary>
    /// Takes input from the left stick's horizontal position, driven by Input Controller
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void LeftStickControl(CallbackContext context)
    {
        leftStickValue = context.ReadValue<float>();
    }

    /// <summary>
    /// Takes input from the right trigger, driven by Input Controller
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void RightTriggerControl(CallbackContext context)
    {
        rightTriggerValue = context.ReadValue<float>();
    }

    /// <summary>
    /// Takes input from the left trigger, driven by Input Controller
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void LeftTriggerControl(CallbackContext context) 
    {
        leftTriggerValue = context.ReadValue<float>();
    }
}

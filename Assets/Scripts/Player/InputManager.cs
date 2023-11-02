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
    public delegate void NorthFaceDelegate(bool northFaceState);
    public event SouthFaceDelegate NorthFaceEvent;

    public delegate void WestFaceDelegate(bool westFaceState);
    public event WestFaceDelegate WestFaceEvent;

    public delegate void SouthFaceDelegate(bool southFaceState);
    public event SouthFaceDelegate SouthFaceEvent;

    private float leftStickValue; //a value from -1 to 1, which represents the horizontal position of the left stick
    public float LeftStickValue { get { return leftStickValue; } }
    private float rightTriggerValue; //a value from 0 to 1, which represents the pull of the right trigger
    public float RightTriggerValue { get { return rightTriggerValue; } }
    private float leftTriggerValue; //a value from 0 to 1, which represents the pull of the left trigger
    public float LeftTriggerValue { get { return leftTriggerValue; } }
    private bool northFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool NorthFaceValue { get { return northFaceValue; } }
    private bool westFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool WestFaceValue { get { return westFaceValue; } }
    private bool southFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool SouthFaceValue { get { return southFaceValue; } }

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

    /// <summary>
    /// Takes input from the north face button (Y on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void NorthFaceTrigger(CallbackContext context)
    {
        northFaceValue = context.ReadValueAsButton();
        NorthFaceEvent(northFaceValue);
    }

    /// <summary>
    /// Takes input from the west face button (X on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void WestFaceTrigger(CallbackContext context) 
    {
        westFaceValue = context.ReadValueAsButton();
        WestFaceEvent(westFaceValue);
    }

    /// <summary>
    /// Takes input from the south face button (A on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void SouthFaceTrigger(CallbackContext context)
    {
        southFaceValue = context.ReadValueAsButton();
        SouthFaceEvent(southFaceValue);
    }
}

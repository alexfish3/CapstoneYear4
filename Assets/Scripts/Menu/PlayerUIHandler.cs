using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerUIHandler : MonoBehaviour
{
    public GameObject MenuCanvas;

    public delegate void NorthFaceDelegate(bool northFaceState);
    public event NorthFaceDelegate NorthFaceEvent;
    private bool northFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool NorthFaceValue { get { return northFaceValue; } }

    public delegate void EastFaceDelegate(bool eastFaceState);
    public event EastFaceDelegate EastFaceEvent;
    private bool eastFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool EastFaceValue { get { return eastFaceValue; } }

    public delegate void SouthFaceDelegate(bool southFaceState);
    public event SouthFaceDelegate SouthFaceEvent;
    private bool southFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool SouthFaceValue { get { return southFaceValue; } }

    public delegate void WestFaceDelegate(bool westFaceState);
    public event WestFaceDelegate WestFaceEvent;
    private bool westFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool WestFaceValue { get { return westFaceValue; } }

    /// <summary>
    /// Takes input from the north face button (Y on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void NorthFaceTrigger(CallbackContext context)
    {
        northFaceValue = context.ReadValueAsButton();
        NorthFaceEvent?.Invoke(northFaceValue);
    }

    /// <summary>
    /// Takes input from the east face button (B on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void EastFaceTrigger(CallbackContext context)
    {
        eastFaceValue = context.ReadValueAsButton();
        EastFaceEvent?.Invoke(eastFaceValue);
    }

    /// <summary>
    /// Takes input from the south face button (A on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void SouthFaceTrigger(CallbackContext context)
    {
        southFaceValue = context.ReadValueAsButton();
        SouthFaceEvent?.Invoke(southFaceValue);
    }

    /// <summary>
    /// Takes input from the west face button (X on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void WestFaceTrigger(CallbackContext context)
    {
        westFaceValue = context.ReadValueAsButton();
        WestFaceEvent?.Invoke(westFaceValue);
    }
}

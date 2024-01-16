using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.InputSystem.InputAction;

public class PlayerUIHandler : MonoBehaviour
{
    public GameObject MenuCanvas;
    public MenuInteractions menuInteractions;
    public float scrollSpeed = 0.2f;

    public float uiDelayTime = 0.1f;
    [SerializeField] bool canInput = false;

    public UnityEvent<bool> NorthFaceEvent;
    private bool northFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool NorthFaceValue { get { return northFaceValue; } }

    public UnityEvent<bool> EastFaceEvent;
    private bool eastFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool EastFaceValue { get { return eastFaceValue; } }

    public UnityEvent<bool> SouthFaceEvent;
    private bool southFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool SouthFaceValue { get { return southFaceValue; } }

    public UnityEvent<bool> WestFaceEvent;
    private bool westFaceValue; //a bool representing the pushed state of the west face button (true for pushed, false for loose)
    public bool WestFaceValue { get { return westFaceValue; } }

    public UnityEvent<bool> LeftPadEvent;
    private bool leftPadValue; //a bool representing the pushed state of the left d-pad button (true for pushed, false for loose)
    public bool LeftPadValue { get { return leftPadValue; } }

    public UnityEvent<bool> RightPadEvent;
    private bool rightPadValue; //a bool representing the pushed state of the right d-pad button (true for pushed, false for loose)
    public bool RightPadValue { get { return rightPadValue; } }

    public UnityEvent<bool> UpPadEvent;
    private bool upPadValue; //a bool representing the pushed state of the up d-pad button (true for pushed, false for loose)
    public bool UpPadValue { get { return upPadValue; } }

    public UnityEvent<bool> DownPadEvent;
    private bool downPadValue; //a bool representing the pushed state of the down d-pad button (true for pushed, false for loose)
    public bool DownPadValue { get { return downPadValue; } }

    public UnityEvent<bool> StartPadEvent;
    private bool startPadValue; // A bool representing the pushed stage of the start button (true for pushed, false for loose)


    public IEnumerator Start()
    {
        canInput = false;
        yield return new WaitForSeconds(uiDelayTime);
        canInput = true;
    }


    /// <summary>
    /// Takes input from the north face button (Y on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void NorthFaceTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        northFaceValue = context.ReadValueAsButton();
        NorthFaceEvent?.Invoke(northFaceValue);
    }

    /// <summary>
    /// Takes input from the east face button (B on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void EastFaceTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        eastFaceValue = context.ReadValueAsButton();
        EastFaceEvent?.Invoke(eastFaceValue);
    }

    /// <summary>
    /// Takes input from the south face button (A on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void SouthFaceTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        southFaceValue = context.ReadValueAsButton();
        SouthFaceEvent?.Invoke(southFaceValue);
    }

    /// <summary>
    /// Takes input from the west face button (X on Xbox)
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void WestFaceTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        westFaceValue = context.ReadValueAsButton();
        WestFaceEvent?.Invoke(westFaceValue);
    }

    /// <summary>
    /// Takes input from the left d-pad button
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void LeftPadTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        leftPadValue = context.ReadValueAsButton();

        if (context.performed)
        {
            LeftPadEvent?.Invoke(leftPadValue);
            StartCoroutine(ScrollPress(context, 0));
        }
    }

    /// <summary>
    /// Takes input from the right d-pad button
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void RightPadTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        rightPadValue = context.ReadValueAsButton();

        if (context.performed)
        {
            RightPadEvent?.Invoke(rightPadValue);
            StartCoroutine(ScrollPress(context, 1));
        }
    }

    /// <summary>
    /// Takes input from the up d-pad button
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void UpPadTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        upPadValue = context.ReadValueAsButton();

        if (context.performed)
        {
            UpPadEvent?.Invoke(upPadValue);
            StartCoroutine(ScrollPress(context, 2));
        }
    }

    /// <summary>
    /// Takes input from the down d-pad button
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void DownPadTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        downPadValue = context.ReadValueAsButton();

        if (context.performed)
        {
            DownPadEvent?.Invoke(downPadValue);
            StartCoroutine(ScrollPress(context, 3));
        }
    }

    // Enables the ability to scoll selection
    private IEnumerator ScrollPress(CallbackContext context, int buttonType)
    {
        yield return new WaitForSeconds(scrollSpeed);

        switch (buttonType)
        {
            case 0: // Left D-Pad
                if (leftPadValue == true)
                {
                    LeftPadTrigger(context);
                }
                break;
            case 1: // Right D-Pad
                if (rightPadValue == true)
                {
                    RightPadTrigger(context);
                }
                break;
            case 2: // Up D-Pad
                if (upPadValue == true)
                {
                    UpPadTrigger(context);
                }
                break;
            case 3: // Down D-Pad
                if (downPadValue == true)
                {
                    DownPadTrigger(context);
                }
                break;
        }
    }

    /// <summary>
    /// Takes input from the start button
    /// </summary>
    /// <param name="context">boilerplate for Input Controller</param>
    public void StartPadTrigger(CallbackContext context)
    {
        // Disables input untill can input is true
        if (canInput == false)
            return;

        startPadValue = context.ReadValueAsButton();

        if (context.performed)
        {
            StartPadEvent?.Invoke(startPadValue);
        }
    }
}

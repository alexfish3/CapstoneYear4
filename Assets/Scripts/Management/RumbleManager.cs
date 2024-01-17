using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : SingletonMonobehaviour<RumbleManager>
{
    private Gamepad pad;

    private IEnumerator pulseTimeCoroutine;

    private void Start()
    {
        Gamepad.current.SetMotorSpeeds(0f, 0f);
        InputSystem.ResetHaptics();
    }

    private void OnApplicationQuit()
    {
        Gamepad.current.SetMotorSpeeds(0f, 0f);
        InputSystem.ResetHaptics();
    }

    public void RumblePulse(float low, float high, float duration)
    {
        pad = Gamepad.current;

        if (pad != null)
        {
            pad.SetMotorSpeeds(low, high);
        }

        StartPulseTime(duration);
    }

    private IEnumerator PulseTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        pad.SetMotorSpeeds(0f, 0f);
    }

    private void StartPulseTime(float duration)
    { 
        pulseTimeCoroutine = PulseTime(duration);
        StartCoroutine(pulseTimeCoroutine);
    }
    private void StopPulseTime()
    {
        StopCoroutine(pulseTimeCoroutine);
        pulseTimeCoroutine = null;
    }
}

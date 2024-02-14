using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : SingletonMonobehaviour<RumbleManager>
{
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

    public void RumblePulse(Gamepad pad, float low, float high, float duration)
    {
        if (pad != null)
        {
            pad.SetMotorSpeeds(low, high);
        }

        StartPulseTime(duration, pad);
    }

    private IEnumerator PulseTime(float duration, Gamepad pad)
    {
        yield return new WaitForSeconds(duration);
        pad.SetMotorSpeeds(0f, 0f);
    }

    private void StartPulseTime(float duration, Gamepad pad)
    {
        pulseTimeCoroutine = PulseTime(duration, pad);
        StartCoroutine(pulseTimeCoroutine);
    }
    private void StopPulseTime()
    {
        StopCoroutine(pulseTimeCoroutine);
        pulseTimeCoroutine = null;
    }
}

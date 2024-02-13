using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialToggle : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    private float toggleCooldown = 0.2f;
    private float toggleTimer = 0f;
    IEnumerator toggleRoutine;

    private void OnEnable()
    {
        GameManager.Instance.OnSwapAnything += EndToggleCooldown;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapAnything -= EndToggleCooldown;
    }

    public void SetTutorial(bool button)
    {
        if (toggleTimer <= 0f)
        {
            toggleTimer = toggleCooldown;
            toggle.isOn = !toggle.isOn;
            TutorialManager.Instance.ShouldTutorialize = toggle.isOn;
            StartToggleCooldown();
        }
    }

    private void StartToggleCooldown()
    {
        if(toggleRoutine == null)
        {
            toggleRoutine = WaitForToggleCooldown();
            StartCoroutine(toggleRoutine);
        }
    }

    private void EndToggleCooldown()
    {
        if(toggleRoutine != null)
        {
            StopCoroutine(toggleRoutine);
            toggleRoutine = null;
        }
        this.gameObject.SetActive(false);
    }

    private IEnumerator WaitForToggleCooldown()
    {
        toggleTimer = toggleCooldown;
        while (toggleTimer > 0f)
        {
            toggleTimer -= Time.deltaTime;
            yield return null;
        }
        toggleRoutine = null;
    }
}

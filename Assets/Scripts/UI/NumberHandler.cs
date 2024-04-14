using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumberHandler : MonoBehaviour
{
    [SerializeField] bool trigger;

    [Header("Sprite Information")]
    [SerializeField] Sprite[] numberSprites;

    [Header("Tip Counter")]
    [SerializeField] string moneyCount;
    [SerializeField] Image[] moneyImages;
    [SerializeField] GameObject tipHolder;
    [SerializeField] GameObject dollarSign;
    [SerializeField] Image placementImage;
    [SerializeField] private TextMeshProUGUI suffix;
    [SerializeField] private TextMeshProUGUI suffixBG;

    [Header("Final Order Countdown")]
    [SerializeField] Image finalOrderCountdownImage;

    private int currPlace = -1;
    // Update is called once per frame
    void Update()
    {
        if (trigger == true)
        {
            trigger = false;
            UpdateScoreUI(moneyCount);
        }
    }

    public void UpdateScoreUI(string passedInScore)
    {
        moneyCount = passedInScore;

        char[] splitScore = passedInScore.ToCharArray();

        //// Reset dollar positions
        //moneyImages[0].enabled = true;
        //moneyImages[1].enabled = true;
        //moneyImages[2].enabled = true;

        // If there is 0-9 dollars
        if (splitScore.Length == 1)
        {
            moneyImages[0].enabled = false;
            moneyImages[1].enabled = false;
            StartCoroutine(animateNumber(moneyImages[2], splitScore[0]));
            dollarSign.transform.localPosition = new Vector3(20, 0, 0);
            tipHolder.transform.localPosition = new Vector3(-50, 0, 0);
            tipHolder.transform.localScale = new Vector3(1.45f, 1.45f, 1.45f);
        }
        // If there is 10-99 dollars
        else if (splitScore.Length == 2)
        {
            moneyImages[0].enabled = false;
            StartCoroutine(animateNumber(moneyImages[1], splitScore[0]));
            StartCoroutine(animateNumber(moneyImages[2], splitScore[1]));
            dollarSign.transform.localPosition = new Vector3(-45, 0, 0);
            tipHolder.transform.localPosition = new Vector3(-16, 0, 0);
            tipHolder.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
        // If there is 100-999 dollars
        else if (splitScore.Length == 3)
        {
            StartCoroutine(animateNumber(moneyImages[0], splitScore[0]));
            StartCoroutine(animateNumber(moneyImages[1], splitScore[1]));
            StartCoroutine(animateNumber(moneyImages[2], splitScore[2]));
            dollarSign.transform.localPosition = new Vector3(-100, 0, 0);

            tipHolder.transform.localPosition = new Vector3(0, 0, 0);
            tipHolder.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void UpdateFinalCountdown(int time)
    {
        StartCoroutine(animateNumber(finalOrderCountdownImage, time));
    }

    public void SetFinalCountdown(bool enabled)
    {
        finalOrderCountdownImage.transform.parent.gameObject.SetActive(enabled);
    }

    // Methods for returning the number sprite I need
    private Sprite getNumSprite(char intNum)
    {
        int i;
        int.TryParse("" + intNum, out i);
        return numberSprites[i];
    }
    private Sprite getNumSprite(int intNum)
    {
        return numberSprites[intNum];
    }

    public void UpdatePlacement(int inPlace)
    {
        if (currPlace == inPlace)
            return;

        currPlace = inPlace;
        StartCoroutine(animateNumber(placementImage, inPlace));

        switch(inPlace)
        {
            case 1:
                suffix.text = "st";
                break;
            case 2:
                suffix.text = "nd";
                break;
            case 3:
                suffix.text = "rd";
                break;
            default:
                suffix.text = "th";
                break;
        }
        suffixBG.text = suffix.text;
    }

    private IEnumerator animateNumber(Image numToShrink, char charNum)
    {
        Sprite spriteToChangeTo = getNumSprite(charNum);
        Animator numAnimator = numToShrink.transform.parent.GetComponent<Animator>();

        numAnimator.SetTrigger(HashReference._shrinkTrigger);
        yield return new WaitForSeconds(0.2f);
        numToShrink.enabled = true;

        numToShrink.sprite = spriteToChangeTo;
        numAnimator.SetTrigger(HashReference._growTrigger);

        yield return new WaitForSeconds(Random.Range(0, 0.4f));
        numAnimator.SetTrigger(HashReference._idleTrigger);
        yield break;
    }

    private IEnumerator animateNumber(Image numToShrink, int intNum)
    {
        Sprite spriteToChangeTo = getNumSprite(intNum);
        Animator numAnimator = numToShrink.transform.parent.GetComponent<Animator>();

        // else animate number
        numAnimator.SetTrigger(HashReference._shrinkTrigger);
        yield return new WaitForSeconds(0.2f);
        numToShrink.enabled = true;

        numToShrink.sprite = spriteToChangeTo;
        numAnimator.SetTrigger(HashReference._growTrigger);

        yield return new WaitForSeconds(Random.Range(0, 0.4f));
        numAnimator.SetTrigger(HashReference._idleTrigger);
        yield break;
    }
}

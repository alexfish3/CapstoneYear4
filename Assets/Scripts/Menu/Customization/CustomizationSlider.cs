using UnityEngine;
using UnityEngine.UI;

public class CustomizationSlider : MonoBehaviour
{
    public int currentMainPos;
    [SerializeField] Sprite[] sliderSprites;
    [SerializeField] int[] imagePositions;

    [SerializeField] Image main;
    [SerializeField] Image left1;
    [SerializeField] Image left2;
    [SerializeField] Image right1;
    [SerializeField] Image right2;

    public void SetSliderSprites(Sprite[] sprites)
    {
        sliderSprites = sprites;
    }

    public void UpdateIcons()
    {
        imagePositions = new int[5];

        // Calculate left2Pos
        imagePositions[0] = (currentMainPos - 2 + sliderSprites.Length) % sliderSprites.Length;

        // Calculate left1Pos
        imagePositions[1] = (currentMainPos - 1 + sliderSprites.Length) % sliderSprites.Length;

        // Calculate Main
        imagePositions[2] = currentMainPos;

        // Calculate right1Pos
        imagePositions[3] = (currentMainPos + 1) % sliderSprites.Length;

        // Calculate right2Pos
        imagePositions[4] = (currentMainPos + 2) % sliderSprites.Length;

        left2.sprite = sliderSprites[imagePositions[0]];
        left1.sprite = sliderSprites[imagePositions[1]];

        main.sprite = sliderSprites[imagePositions[2]];

        right1.sprite = sliderSprites[imagePositions[3]];
        right2.sprite = sliderSprites[imagePositions[4]];
    }

    public void UpdateSliderIcons(Sprite mainSprite, Sprite left1Sprite, Sprite left2Sprite, Sprite right1Sprite, Sprite right2Sprite)
    {
        main.sprite = mainSprite;
        left1.sprite = left1Sprite;
        left2.sprite = left2Sprite;
        right1.sprite = right1Sprite;
        right2.sprite = right2Sprite;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : SingletonMonobehaviour<OptionsMenu>
{
    [Header("Options Numbers")]
    [SerializeField] float[] volumeValues;

    [Header("BGM Info")]
    [SerializeField] GameObject bgmSelector;
    [SerializeField] GameObject[] bgmSelectorPositions;
    [SerializeField] float bgmValue;
    [SerializeField] int bgmPosition;

    [Header("SFX Info")]
    [SerializeField] GameObject sfxSelector;
    [SerializeField] GameObject[] sfxSelectorPositions;
    [SerializeField] float sfxValue;
    [SerializeField] int sfxPosition;

    public enum OptionSelected
    {
        BGM = 0,
        SFX = 1,
        FULLSCREEN = 2
    }
    public OptionSelected optionSelected;

    [Header("Selector Objects")]
    [SerializeField] GameObject selector;
    [SerializeField] GameObject[] selectorObjects;
    int selectorPos;
    [SerializeField] MainMenu menu;

    ///<summary>
    /// Saves the game's option settings
    ///</summary>
    public void SaveOptions()
    {

    }

    ///<summary>
    /// Loads the game's option settings
    ///</summary>
    public void LoadOptions()
    {

    }

    ///<summary>
    /// Main method which allows users to scroll up/ down
    ///</summary>
    public void ScrollMenuUpDown(bool direction)
    {
        // Positive Scroll
        if (direction)
        {
            if (selectorPos == selectorObjects.Length - 1)
            {
                selectorPos = 0;
            }
            else
            {
                selectorPos = selectorPos + 1;
            }
        }
        // Negative Scroll
        else
        {
            if (selectorPos == 0)
            {
                selectorPos = selectorObjects.Length - 1;
            }
            else
            {
                selectorPos = selectorPos - 1;
            }
        }

        optionSelected = (OptionSelected)selectorPos;

        // Updates selector for current slider selected
        selector.transform.position = new Vector3(selector.transform.position.x, selectorObjects[selectorPos].transform.position.y, selector.transform.position.z);
    }

    ///<summary>
    /// Main method which allows users to scroll 
    ///</summary>
    public void ScrollMenuLeftRight(bool direction)
    {
        if(optionSelected == OptionSelected.BGM)
        {
            // Positive Scroll
            if (direction)
            {
                if (bgmPosition == bgmSelectorPositions.Length - 1)
                {
                    bgmPosition = 0;
                }
                else
                {
                    bgmPosition = bgmPosition + 1;
                }
            }
            // Negative Scroll
            else
            {
                if (bgmPosition == 0)
                {
                    bgmPosition = bgmSelectorPositions.Length - 1;
                }
                else
                {
                    bgmPosition = bgmPosition - 1;
                }
            }

            bgmSelector.transform.position = new Vector3(bgmSelectorPositions[bgmPosition].transform.position.x, bgmSelector.transform.position.y, bgmSelector.transform.position.z);
            bgmValue = volumeValues[bgmPosition];
        }
        else if (optionSelected == OptionSelected.SFX)
        {
            // Positive Scroll
            if (direction)
            {
                if (sfxPosition == sfxSelectorPositions.Length - 1)
                {
                    sfxPosition = 0;
                }
                else
                {
                    sfxPosition = sfxPosition + 1;
                }
            }
            // Negative Scroll
            else
            {
                if (sfxPosition == 0)
                {
                    sfxPosition = sfxSelectorPositions.Length - 1;
                }
                else
                {
                    sfxPosition = sfxPosition - 1;
                }
            }

            sfxSelector.transform.position = new Vector3(sfxSelectorPositions[sfxPosition].transform.position.x, sfxSelector.transform.position.y, sfxSelector.transform.position.z);
            sfxValue = volumeValues[sfxPosition];
        }
        else if(optionSelected == OptionSelected.FULLSCREEN)
        {

        }

        // Positive Scroll
        if (direction)
        {

        }
        // Negative Scroll
        else
        {

        }
    }

    ///<summary>
    /// Exits the options of the main menu
    ///</summary>
    public void ExitMenu()
    {
        Debug.Log("Exit Menu");

        menu.SwapToMainMenu();
        selectorPos = 0;
        optionSelected = 0;
        selector.transform.position = new Vector3(selector.transform.position.x, selectorObjects[selectorPos].transform.position.y, selector.transform.position.z);
    }
}

[Serializable]
public class OptionsSave
{
    public int bgmPositionSave;
    public int sfxPositionSave;
    public int fullscreenSave;
}
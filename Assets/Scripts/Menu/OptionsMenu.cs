using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

    SoundManager soundManager;

    private void Start()
    {
        soundManager = SoundManager.Instance;
        LoadOptions();
    }

    ///<summary>
    /// Saves the game's option settings
    ///</summary>
    public void SaveOptions()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        // checks if file already exists
        if(File.Exists(Application.persistentDataPath + "/settings.dat"))
        {
            file = File.Open(Application.persistentDataPath + "/settings.dat", FileMode.Open);
        }
        else
        {
            file = File.Create(Application.persistentDataPath + "/settings.dat");
        }

        OptionsSave data = new OptionsSave();

        data.bgmPositionSave = bgmPosition;
        data.sfxPositionSave = sfxPosition;

        bf.Serialize(file, data);
        file.Close();
    }

    ///<summary>
    /// Loads the game's option settings
    ///</summary>
    public void LoadOptions()
    {
        // Determines if save file exists to load
        if (File.Exists(Application.persistentDataPath + "/settings.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/settings.dat", FileMode.Open);

            OptionsSave data = (OptionsSave)bf.Deserialize(file);

            // Clamps loaded values incase data was tampered with
            bgmPosition = Mathf.Clamp(data.bgmPositionSave, 0, 11);
            sfxPosition = Mathf.Clamp(data.sfxPositionSave, 0, 11);

            // Closes file reader
            file.Close();
        }

        // Loads values as either loaded values or defualt values
        bgmValue = volumeValues[bgmPosition];
        soundManager.SetMusic(bgmValue);

        sfxValue = volumeValues[sfxPosition];
        soundManager.SetSFX(sfxValue);
    }


    ///<summary>
    /// Updates the selector graphics for the options
    ///</summary>
    public void UpdateSelectors()
    {
        bgmSelector.transform.position = new Vector3(bgmSelectorPositions[bgmPosition].transform.position.x, bgmSelector.transform.position.y, bgmSelector.transform.position.z);
        sfxSelector.transform.position = new Vector3(sfxSelectorPositions[sfxPosition].transform.position.x, sfxSelector.transform.position.y, sfxSelector.transform.position.z);
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
            soundManager.SetMusic(bgmValue);
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
            soundManager.SetSFX(sfxValue);
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

        SaveOptions();

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
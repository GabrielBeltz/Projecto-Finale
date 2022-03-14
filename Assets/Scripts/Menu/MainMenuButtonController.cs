using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuButtonController : MonoBehaviour
{
    public GameObject optionsPanel, backToMenu, crewPanel;
    [SerializeField] private bool isOptionsOn;

    private void Start()
    {
        isOptionsOn = false;
        optionsPanel.SetActive(isOptionsOn);
        crewPanel.SetActive(false);
    }

    public void PlayButton()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        SceneManager.LoadScene(1);
    }

    #region MainButtons

    public void OptionsButton()
    {
        isOptionsOn = !isOptionsOn;

        if(isOptionsOn)
        {
            isOptionsOn = true;
            optionsPanel.SetActive(isOptionsOn);
        }

        optionsPanel.SetActive(isOptionsOn);
    }

    public void CrewButton(bool OnOrOff)
    {
        crewPanel.SetActive(OnOrOff);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    #endregion

    #region EventTriggerSounds

    public void ButtonEnter()
    {
        FindObjectOfType<AudioManager>().Play("ButtonEnter");
    }

    public void ButtonClick()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
    }

    #endregion
}

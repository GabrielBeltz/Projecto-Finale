using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Toggle fullscreenToogle;
    public Dropdown resolutionDropdown, textureQualityDropDown;
    public Slider volumeSlider;
    public AudioMixer audioMixer;
    public Button applyButton;

    public Resolution[] resolutions;
    public GameSettings gameSettings;

    private void Awake()
    {
        if(File.Exists(Application.persistentDataPath + "/gamesettingsV.json"))
        {
            Debug.Log("File Founded");
        }
        else
        {
            File.Create(Application.persistentDataPath + "/gamesettingsV.json");
        }
    }

    private void Start()
    {
        audioMixer.SetFloat("VolumeExposed", gameSettings.volumeSlider);

        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for(int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        LoadingSettings();
    }

    private void OnEnable()
    {
        gameSettings = new GameSettings();
        fullscreenToogle.onValueChanged.AddListener(delegate { OnFullscreenToggle(); });
        resolutionDropdown.onValueChanged.AddListener(delegate { OnResolutionChange(); });
        textureQualityDropDown.onValueChanged.AddListener(delegate { OnTextureQualityChange(); });
        volumeSlider.onValueChanged.AddListener(delegate { OnMusicVolumeChange(); });
        applyButton.onClick.AddListener(delegate { OnApplyButtonClick(); });
        resolutions = Screen.resolutions;

        LoadingSettings();
    }

    public void OnFullscreenToggle()
    {
        gameSettings.fullscreen = Screen.fullScreen = fullscreenToogle.isOn;
    }

    public void OnResolutionChange()
    {
        Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, Screen.fullScreen);
        gameSettings.resolutionIndex = resolutionDropdown.value;
    }

    public void OnTextureQualityChange()
    {
        QualitySettings.masterTextureLimit = gameSettings.textureQuality = textureQualityDropDown.value;
    }

    public void OnMusicVolumeChange()
    {
        gameSettings.volumeSlider = volumeSlider.value;
        if(GameObject.Find("AudioController") != null)
        {
            AudioManager.instance.UpdateVolume(gameSettings.volumeSlider);
            Debug.Log("Audio Controller Founded");
        }
        
        audioMixer.SetFloat("VolumeExposed", gameSettings.volumeSlider);
    }

    public void SaveSettings()
    {
        string jsonData = JsonUtility.ToJson(gameSettings, true);
        File.WriteAllText(Application.persistentDataPath + "/gamesettingsV.json", jsonData);
    }

    public void OnApplyButtonClick()
    {
        SaveSettings();
    }

    public void LoadingSettings()
    {
        GameSettings jsonData = JsonUtility.FromJson<GameSettings>(File.ReadAllText(Application.persistentDataPath + "/gamesettingsV.json"));
        fullscreenToogle.isOn = jsonData.fullscreen;
        resolutionDropdown.value = jsonData.resolutionIndex;
        textureQualityDropDown.value = jsonData.textureQuality;
        volumeSlider.value = jsonData.volumeSlider;
        audioMixer.SetFloat("VolumeExposed", jsonData.volumeSlider);
        resolutionDropdown.RefreshShownValue();
        Screen.SetResolution(resolutions[jsonData.resolutionIndex].width, resolutions[jsonData.resolutionIndex].height, Screen.fullScreen);
    }
}
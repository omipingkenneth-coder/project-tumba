using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider masterVolumeSlider;      // Slider for adjusting master volume
    public Slider musicVolumeSlider;       // Slider for adjusting music volume
    public Slider sfxVolumeSlider;         // Slider for adjusting sound effects volume

    [Header("Graphics Settings")]
    public Dropdown qualityDropdown;       // Dropdown for graphics quality levels
    public Dropdown resolutionDropdown;    // Dropdown for screen resolutions
    public Toggle fullscreenToggle;        // Toggle for fullscreen mode

    [Header("Apply Button")]
    public Button applyButton;             // Button to apply the settings

    // Store the default values
    private float defaultMasterVolume;
    private float defaultMusicVolume;
    private float defaultSFXVolume;
    private int defaultQuality;
    private int defaultResolutionIndex;
    private bool defaultFullscreen;

    void Start()
    {
        // Load current settings
        LoadSettings();

        // Set up the UI with the saved settings
        masterVolumeSlider.value = defaultMasterVolume;
        musicVolumeSlider.value = defaultMusicVolume;
        sfxVolumeSlider.value = defaultSFXVolume;

        qualityDropdown.value = defaultQuality;
        fullscreenToggle.isOn = defaultFullscreen;
        resolutionDropdown.value = defaultResolutionIndex;

        // Set listeners for UI controls
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        // Apply button listener
        applyButton.onClick.AddListener(ApplySettings);
    }

    void LoadSettings()
    {
        // Load saved settings (you can use PlayerPrefs or a settings manager in a more complex game)
        defaultMasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        defaultMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        defaultSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        defaultQuality = PlayerPrefs.GetInt("Quality", 2); // Default: Medium
        defaultResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0); // Default: First resolution
        defaultFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1; // Default: Fullscreen ON
    }

    void ApplySettings()
    {
        // Apply audio settings
        AudioListener.volume = masterVolumeSlider.value;
        // Assuming you have separate AudioSource for Music and SFX
        // You can add a Music and SFX AudioSource references to control them separately.
        // MusicVolume, SFXVolume could be controlled through individual audio sources in the scene.

        // Apply Graphics Settings
        QualitySettings.SetQualityLevel(qualityDropdown.value);

        // Apply Resolution and Fullscreen
        Resolution resolution = Screen.resolutions[resolutionDropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle.isOn);

        // Save settings
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetInt("Quality", qualityDropdown.value);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Settings Applied.");
    }

    // Event Handlers (Optional for individual setting change)
    void OnMasterVolumeChanged(float value)
    {
        // Handle master volume change if needed
    }

    void OnMusicVolumeChanged(float value)
    {
        // Handle music volume change if needed
    }

    void OnSFXVolumeChanged(float value)
    {
        // Handle SFX volume change if needed
    }

    void OnQualityChanged(int value)
    {
        // Handle quality level change if needed
    }

    void OnResolutionChanged(int value)
    {
        // Handle resolution change if needed
    }

    void OnFullscreenChanged(bool isFullscreen)
    {
        // Handle fullscreen toggle change if needed
    }
}

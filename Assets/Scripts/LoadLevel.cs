using System;
using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    public int levelToLoad;
    public DontDestroySettings dontDestroySettings;
    public bool isMultiplayer = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void LoadLevelByIndex()
    {
        dontDestroySettings = GameObject.FindWithTag("Settings").GetComponent<DontDestroySettings>();
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelToLoad);
        dontDestroySettings.isMultiplayer = isMultiplayer;

    }
}

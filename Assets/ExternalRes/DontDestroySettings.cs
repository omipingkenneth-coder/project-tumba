using UnityEngine;
using System.Collections.Generic;

public class DontDestroySettings : MonoBehaviour
{
    public List<string> offlineScenes;
    public List<string> onlineScenes;
    public bool isMultiplayer = false;
    public int selectedCharacterIndex = 0;
    public int selectedOfflineScenesIndex = 0;
    public int selectedOnlineScenesIndex = 0;
    public int PlayerID;
    public string PlayerName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}

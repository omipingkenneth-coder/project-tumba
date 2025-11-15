using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager
{
    [Header("UI References")]
    public Button hostButton;
    public Button clientButton;
    public InputField addressInput;
    public Text statusText;

    [Header("Player Prefabs")]
    public GameObject playerPrefabA; // normal player
    public GameObject playerPrefabB; // special player

    [Header("Spawn Points")]
    public Transform spawnPointA;
    public Transform spawnPointB;

    private bool specialPlayerAssigned = false;
    private int playerCount = 0;
    public GameObject UIPanel;

    public override void Awake()
    {
        base.Awake();
        if (hostButton) hostButton.onClick.AddListener(StartHostConnection);
        if (clientButton) clientButton.onClick.AddListener(StartClientConnection);
    }

    void StartHostConnection()
    {
        statusText.text = "Starting Host...";
        autoCreatePlayer = false;
        StartHost();
    }

    void StartClientConnection()
    {
        if (addressInput && !string.IsNullOrEmpty(addressInput.text))
            networkAddress = addressInput.text;

        statusText.text = $"Connecting to {networkAddress}...";
        StartClient();
        UIPanel.SetActive(false);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        statusText.text = "Client connected!";
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (!NetworkServer.active) return;

        GameObject prefabToSpawn;
        Vector3 spawnPosition;

        if (!specialPlayerAssigned)
        {
            prefabToSpawn = playerPrefabB;
            spawnPosition = spawnPointB.position;
            specialPlayerAssigned = true;
            Debug.Log("Special Player spawned at B.");
        }
        else
        {
            prefabToSpawn = playerPrefabA;
            float offsetRadius = 2f;
            float angle = playerCount * 45f * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * offsetRadius;
            spawnPosition = spawnPointA.position + offset;
        }

        GameObject player = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);

        playerCount++;
        UpdateAllClientPlayerLists();
    }



    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        statusText.text = "Client disconnected.";
        UpdateAllClientPlayerLists();
    }

    private void UpdateAllClientPlayerLists()
    {
        // Build a list of all connected player names
        var players = FindObjectsOfType<NetworkPlayerProperties>();
        string allNames = string.Join("\n", players.Select(p => p.PlayerName).ToArray());

        // Notify all clients through the helper
        if (NetworkPlayerListSync.Instance != null)
        {
            NetworkPlayerListSync.Instance.RpcUpdatePlayerList(allNames);
        }
    }

    public override void OnStopHost()
    {
        specialPlayerAssigned = false;
        playerCount = 0;
        statusText.text = "Host stopped.";
    }
}

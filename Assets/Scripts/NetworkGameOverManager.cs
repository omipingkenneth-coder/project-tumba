using UnityEngine;
using Mirror;

public class NetworkGameOverManager : NetworkBehaviour
{
    [Header("Settings")]
    public string gameOverPanelTag = "GameOverPanel"; // Tag to find the panel
    public float delay = 5f;
    public bool globalGameOver = false; // true = show to all players, false = only local player

    private GameObject gameOverPanel;
    private bool isGameOver = false;

    private void Start()
    {
        // Try to find the panel in the scene by tag
        gameOverPanel = GameObject.FindGameObjectWithTag(gameOverPanelTag);
        if (gameOverPanel == null)
        {
            Debug.LogWarning("GameOver panel not found! Check the tag.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; // Only server handles game logic

        if (other.CompareTag("Player") && !isGameOver)
        {
            isGameOver = true;

            // Disable movement scripts on server
            var movement = other.GetComponent<NetworkCharacterControllerMovement>();
            if (movement != null) movement.enabled = false;

            var pickThrow = other.GetComponent<NetworkPickAndThrow>();
            if (pickThrow != null) pickThrow.enabled = false;

            // Show GameOver UI
            if (globalGameOver)
            {
                RpcShowGameOverUI(); // All clients
            }
            else
            {
                TargetShowGameOverUI(other.GetComponent<NetworkIdentity>().connectionToClient);
            }

            Invoke(nameof(GameOverNextScene), delay);
        }
    }

    // ---------------- UI ----------------

    // Shown on all clients
    [ClientRpc]
    void RpcShowGameOverUI()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // Shown only on a specific client
    [TargetRpc]
    void TargetShowGameOverUI(NetworkConnection target)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    // ---------------- Scene Management ----------------

    void GameOverNextScene()
    {
        if (NetworkManager.singleton != null)
        {
            NetworkManager.singleton.ServerChangeScene("MainMenu");
        }
        else
        {
            Debug.LogWarning("NetworkManager singleton not found!");
        }
    }
}

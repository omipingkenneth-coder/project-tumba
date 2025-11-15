using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;   // Assign in Inspector
    public string sceneToLoad = "MainMenu"; // Scene to load after 5 seconds
    public float delay = 5f;

    bool isGameOver = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameOver();

            // Disable player movement scripts
            CharacterControllerMovement movement = other.GetComponent<CharacterControllerMovement>();
            if (movement != null) movement.enabled = false;

            PickandThrow pickThrow = other.GetComponent<PickandThrow>();
            if (pickThrow != null) pickThrow.enabled = false;
        }
    }

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        gameOverPanel.SetActive(true);   // Show UI Panel
        Invoke(nameof(LoadNextScene), delay);  // Load scene after 5 seconds
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}

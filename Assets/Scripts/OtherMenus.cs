using UnityEngine;
using UnityEngine.UI;
public class OtherMenus : MonoBehaviour
{
    public GameObject ExitPanel;
    public GameObject TopScoresPanel;
    public GameObject GameOverPanel;
    public Text TopScores;
    public void ShowExit()
    {
        TopScoresPanel.SetActive(false);
        GameOverPanel.SetActive(false);
        ExitPanel.SetActive(true);
    }
    public void ShowTopScores()
    {
        ExitPanel.SetActive(false);
        TopScoresPanel.SetActive(true);
        GameOverPanel.SetActive(false);
        TopScores.text = PlayerPrefs.GetString("TopScores", "No scores yet.");
    }
    public void Exit()
    {
        Application.Quit();
    }
}

using UnityEngine;

public class CloseUI : MonoBehaviour
{
    public GameObject prevCamera;
    public GameObject uiPanel;
    public void ClosePanel()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
        Destroy(prevCamera);
    }
}

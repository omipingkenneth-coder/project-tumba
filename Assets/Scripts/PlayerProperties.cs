using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerProperties : MonoBehaviour
{
    public string PlayerID = "001";
    public string PlayerName = "Player";
    public int PlayerScore = 0;
    public int PlayerExp = 100;
    public int maxExp = 100;
    public int PlayerStamina = 100;
    public int maxStamina = 100;
    public int Coins = 0;
    public float AddPlayerSpeed = 0;

    [Header("UI References")]
    public GameObject UIPanelPlayer;
    public Text txtPanelScore;
    public Text txtScore;
    public Text txtStamina;
    public Text txtExp;
    public Text txtPlayerName;
    public Text txtCoin;

    private Coroutine hidePanelCoroutine; // To manage panel visibility timing

    void Start()
    {
        if (txtPlayerName != null)
        {
            txtPlayerName.text = PlayerName;
        }
        UpdateUIPlayerPanel();
    }
    public void UpdateExp()
    {
        if (PlayerExp >= maxExp)
        {
            PlayerExp = PlayerExp - maxExp;
            maxExp += 50;
            AddPlayerSpeed += 0.2f;
            maxStamina += 10;
            PlayerStamina = maxStamina;
        }
        else
        {
            PlayerExp += 10;
        }

        UpdateUIPlayerPanel();
    }

    public void UpdateScore()
    {
        PlayerScore += 1;
        Coins += Random.Range(10, 20);
        UpdateUIPlayerPanel();
        UpdateExp();

        // Show the panel for 10 seconds
        if (UIPanelPlayer != null)
        {
            txtPanelScore.text = "Additional Score: " + PlayerScore + "\nTotal Score: " + PlayerScore;
            // Cancel any previous coroutine so timing resets on every score update
            if (hidePanelCoroutine != null)
                StopCoroutine(hidePanelCoroutine);

            UIPanelPlayer.SetActive(true);
            hidePanelCoroutine = StartCoroutine(HidePanelAfterDelay(5f));
        }
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UIPanelPlayer.SetActive(false);
    }

    void UpdateUIPlayerPanel()
    {
        if (txtScore != null)
            txtScore.text = "Score: " + PlayerScore;

        if (txtExp != null)
            txtExp.text = "Exp: " + PlayerExp;

        if (txtStamina != null)
            txtStamina.text = "Stamina: " + PlayerStamina;

        if (txtCoin != null)
            txtCoin.text = "Coins: " + Coins;

    }
}

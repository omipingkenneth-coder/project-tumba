using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

public class NetworkPlayerProperties : NetworkBehaviour
{
    [Header("Player Info")]
    [SyncVar(hook = nameof(OnPlayerIDChanged))] public string PlayerID = "001";
    [SyncVar(hook = nameof(OnPlayerNameChanged))] public string PlayerName = "Player";
    [SyncVar(hook = nameof(OnScoreChanged))] public int PlayerScore = 0;
    [SyncVar(hook = nameof(OnExpChanged))] public int PlayerExp = 100;
    [SyncVar] public int maxExp = 100;
    [SyncVar(hook = nameof(OnStaminaChanged))] public int PlayerStamina = 100;
    [SyncVar] public int maxStamina = 100;
    [SyncVar] public float AddPlayerSpeed = 0;

    [Header("UI References (Local Only)")]
    public GameObject UIPanelPlayer;
    public Text txtPanelScore;
    public Text txtScore;
    public Text txtStamina;
    public Text txtExp;
    public Text txtPlayerName;
    public Text txtCoin;
    public int Coins = 0;

    [Header("3D World UI")]
    public TextMesh worldText;
    public Vector3 worldTextOffset = new Vector3(0, 2f, 0);

    private Camera mainCamera;
    private Coroutine hidePanelCoroutine;

    // -------------------- UNITY EVENTS --------------------
    public override void OnStartClient()
    {
        base.OnStartClient();
        UpdateUIPlayerPanel();
        UpdateWorldText();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (!isLocalPlayer) return;

        // Setup Joystick / Buttons
        var joysticks = GameObject.FindGameObjectsWithTag("JoystickMove");
        foreach (var joystick in joysticks)
        {
            var joystickScript = joystick.GetComponent<NetworkUIVirtualJoystick>();
            if (joystickScript != null)
                joystickScript.characterControllerMovement = GetComponent<NetworkCharacterControllerMovement>();
        }

        var controlActions = GameObject.FindGameObjectsWithTag("ControlAction");
        foreach (var controlAction in controlActions)
        {
            var button = controlAction.GetComponent<UIVirtualButton>();
            if (button != null)
            {
                button.NetCharacterControllerMovement = GetComponent<NetworkCharacterControllerMovement>();
                button.NetworkPickAndThrow = GetComponent<NetworkPickAndThrow>();
            }
        }

        // Setup Camera & UI
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            var cameraFollow = mainCamera.GetComponent<TopDownCameraFollow>();
            if (cameraFollow != null)
                cameraFollow.target = transform;

            var fpsUI = mainCamera.GetComponent<TurnFPSOnOff>();
            if (fpsUI != null)
            {
                UIPanelPlayer = fpsUI.UIPanelPlayer;
                txtPanelScore = fpsUI.txtPanelScore;
                txtScore = fpsUI.txtScore;
                txtStamina = fpsUI.txtStamina;
                txtExp = fpsUI.txtExp;
                txtPlayerName = fpsUI.txtPlayerName;
                txtCoin = fpsUI.txtCoins;

                if (txtPlayerName != null)
                    txtPlayerName.text = PlayerName;
            }
        }

        UpdateUIPlayerPanel();
        Debug.Log("[NetworkPlayerSetup] Local player camera + UI linked successfully.");
    }

    void Update()
    {
        // Update world text position
        if (worldText != null)
        {
            worldText.transform.position = transform.position + worldTextOffset;

            if (mainCamera != null)
                worldText.transform.rotation = Quaternion.LookRotation(worldText.transform.position - mainCamera.transform.position);
        }
    }

    // -------------------- SCORE / EXP --------------------
    [Command]
    public void CmdUpdateExp()
    {
        if (PlayerExp >= maxExp)
        {
            PlayerExp -= maxExp;
            maxExp += 50;
            AddPlayerSpeed += 0.2f;
            maxStamina += 10;
            PlayerStamina = maxStamina;
        }
        else
        {
            PlayerExp += 10;
        }
    }

    [Command]
    public void CmdUpdateScore()
    {
        PlayerScore += 1;
        Coins += Random.Range(10, 20);
        CmdUpdateExp();
    }

    // -------------------- UI PANEL DISPLAY --------------------
    [ClientRpc]
    void RpcShowScorePanel()
    {
        if (!isLocalPlayer) return;

        if (UIPanelPlayer == null) return;

        if (txtPanelScore != null)
            txtPanelScore.text = $"Additional Score: {PlayerScore}\nTotal Score: {PlayerScore}";

        if (hidePanelCoroutine != null)
            StopCoroutine(hidePanelCoroutine);

        UIPanelPlayer.SetActive(true);
        hidePanelCoroutine = StartCoroutine(HidePanelAfterDelay(5f));
    }

    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (UIPanelPlayer != null)
            UIPanelPlayer.SetActive(false);
    }

    // -------------------- UI UPDATE --------------------
    void UpdateUIPlayerPanel()
    {
        if (!isLocalPlayer) return;

        if (txtScore != null) txtScore.text = "Score: " + PlayerScore;
        if (txtExp != null) txtExp.text = $"Exp: {PlayerExp}/{maxExp}";
        if (txtStamina != null) txtStamina.text = $"Stamina: {PlayerStamina}/{maxStamina}";
        if (txtPlayerName != null) txtPlayerName.text = PlayerName;
        if (txtCoin != null) txtCoin.text = "Coins: " + Coins;
    }

    void UpdateWorldText()
    {
        if (worldText == null) return;
        worldText.text = $"ID: {PlayerID}\nName: {PlayerName}\nScore: {PlayerScore}";

        if (mainCamera != null)
            worldText.transform.rotation = Quaternion.LookRotation(worldText.transform.position - mainCamera.transform.position);
    }

    // -------------------- HOOKS --------------------
    void OnPlayerIDChanged(string oldVal, string newVal)
    {
        PlayerID = newVal;
        UpdateWorldText();
    }

    void OnPlayerNameChanged(string oldVal, string newVal)
    {
        PlayerName = newVal;
        UpdateWorldText();
        UpdateUIPlayerPanel();
    }

    void OnScoreChanged(int oldScore, int newScore)
    {
        PlayerScore = newScore;
        UpdateWorldText();
        UpdateUIPlayerPanel();
        RpcShowScorePanel();
    }

    void OnExpChanged(int oldExp, int newExp)
    {
        PlayerExp = newExp;
        UpdateUIPlayerPanel();
    }

    void OnStaminaChanged(int oldSta, int newSta)
    {
        PlayerStamina = newSta;
        UpdateUIPlayerPanel();
    }

    // -------------------- COMMANDS --------------------
    [Command] public void CmdSetPlayerName(string newName) => PlayerName = newName;

    [Command] public void CmdAddScore(int value)
    {
        PlayerScore += value;
        RpcShowScorePanel();
    }

    [Command] public void CmdUseStamina(int value)
    {
        PlayerStamina = Mathf.Max(PlayerStamina - value, 0);
    }

    [Command] public void CmdRestoreStamina(int value)
    {
        PlayerStamina = Mathf.Min(PlayerStamina + value, maxStamina);
    }
}

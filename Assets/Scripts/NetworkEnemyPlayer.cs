using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class NetworkEnemyPlayer : NetworkBehaviour
{
    [Header("References")]
    public CanTrigger canTrigger;
    public Transform holdPoint;         // child under hand
    public GameObject heldCan = null;
    public Transform[] targets;         // Player transforms
    public Transform returnToPosition;  // Return position if idle
    public float followSpeed = 3.0f;
    public float stoppingDistance = 2.0f;
    public float rotationSpeed = 5.0f;
    public float crossFadeDuration = 0.2f;

    private CharacterController characterController;
    private Animation animationComponent;

    private PickandThrow playerPickandThrow;
    private bool isTargetInSafeZone = false;
    private bool isPickingUp = false;
    private bool isHoldingCan = false;
    private bool hasPlacedCan = false;


    public GameObject OnTriggerActivator;
    public GameObject StatusBarPanel;
    public Image StatusBarPanelImage;
    public Text StatusBarText;

    // Run only on server

    private void Start()
    {
        StartCoroutine(UpdateTargetsRoutine());
    }
    IEnumerator UpdateTargetsRoutine()
    {
        while (true)
        {
            RefreshPlayerTargets();
            yield return new WaitForSeconds(5f); // update every second
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();

        characterController = GetComponent<CharacterController>();
        animationComponent = GetComponent<Animation>();

        StatusBarPanel = GameObject.FindGameObjectWithTag("EnemyStatusBar");

        Text[] childTexts = StatusBarPanel.GetComponentsInChildren<Text>(true); // true = include inactive objects
        StatusBarPanelImage = StatusBarPanel.GetComponent<Image>();
        foreach (Text t in childTexts)
        {
            Debug.Log("Found text: " + t.text);

            StatusBarText = t;
        }
        StatusBarText.text = "Welcome! You Are Enemy Player";

        if (animationComponent == null)
            Debug.LogError("No Animation component found on Enemy Player!");

        DebugAnimationClips();
    }
    public void RefreshPlayerTargets()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        targets = new Transform[playerObjects.Length];

        for (int i = 0; i < playerObjects.Length; i++)
        {
            targets[i] = playerObjects[i].transform;
        }
    }

    void Update()
    {
        // Only the server controls AI logic
        if (!isServer) return;

        // Step 1: Pick up the can if needed
        if (heldCan == null && !canTrigger.isCanPositioned)
        {
            //Disable Ontrigger
            AllowAndShowOntriggerMessages(false, "You must pickup the can and set to exact position.");
            // FollowCanPickup();
            return;
        }

        // Step 2: Carry can back to spawn
        if (isHoldingCan && !hasPlacedCan)
        {
            AllowAndShowOntriggerMessages(false, "You must place the can to exact position.");
            // MoveToCanSpawn();
            return;
        }

        // Step 3: Return to base after placing can
        // if (hasPlacedCan)
        // {
        //     AllowAndShowOntriggerMessages(true, "");
        //     ReturnToPosition();
        //     return;
        // }

        // Step 4: No players? Do nothing
        if (targets == null || targets.Length == 0) return;

        // Step 5: Return to base if everyone is safe
        if (AllPlayersInSafeZone())
        {
            AllowAndShowOntriggerMessages(false, "All players are in safe zone");
            //  ReturnToPosition();
            return;
        }

        AllowAndShowOntriggerMessages(true, "Some player is out of safe zone");
        //Set Status to Chase Player and Enable red Arrow of player not in safezone
    }

    void AllowAndShowOntriggerMessages(bool isSetActive, string message)
    {
        OnTriggerActivator.SetActive(isSetActive);
        StatusBarPanel.SetActive(true);
        StatusBarText.text = message;
        if (!isSetActive)
        {
            // StatusBarText Color Red
            StatusBarPanelImage.color = Color.red;
        }
        else
        {
            // StatusBarText Color Green
            StatusBarPanelImage.color = Color.green;
        }
        StartCoroutine(HidePanelAfterDelay(5f));
    }


    private IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StatusBarPanel.SetActive(false);
    }



    bool AllPlayersInSafeZone()
    {
        bool allSafe = true;

        foreach (Transform player in targets)
        {
            if (player == null)
                continue;

            var pick = player.GetComponent<NetworkPickAndThrow>();
            if (pick == null)
                continue; // << SAFE: skip players without the component

            // If player is not in safe zone
            if (!pick.isOnSafeZone)
            {
                pick.ArrowAbove.SetActive(true);
                allSafe = false;
            }
            else
            {
                pick.ArrowAbove.SetActive(false);
            }
        }

        return allSafe;
    }


    // ---------------- FOLLOW PLAYER ----------------

    // ---------------- CAN PICKUP ----------------
    void FollowCanPickup()
    {
        if (isHoldingCan) return;

        Vector3 dir = (canTrigger.Can.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        characterController.Move(dir * followSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, canTrigger.Can.position) < stoppingDistance)
        {
            TryPickupCan(canTrigger.Can.GetComponent<Collider>());
        }

        RpcCrossFadeAnimation("RunningAnimation");
    }

    [Server]
    public void TryPickupCan(Collider canCollider)
    {
        if (heldCan != null) return;

        heldCan = canCollider.gameObject;
        heldCan.transform.SetParent(holdPoint);
        heldCan.transform.localPosition = Vector3.zero;
        heldCan.transform.localRotation = Quaternion.identity;

        var rb = heldCan.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        isHoldingCan = true;
        hasPlacedCan = false;

        RpcCrossFadeAnimation("Pickup");
    }

    // ---------------- PLACE CAN ----------------
    void MoveToCanSpawn()
    {
        Vector3 dir = (canTrigger.CanSpawnPoint.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        characterController.Move(dir * followSpeed * Time.deltaTime);

        RpcCrossFadeAnimation("RunningAnimation");

        if (Vector3.Distance(transform.position, canTrigger.CanSpawnPoint.position) <= stoppingDistance)
        {
            PlaceCanBack();
        }
    }

    [Server]
    void PlaceCanBack()
    {
        if (heldCan == null) return;

        heldCan.transform.SetParent(null);
        heldCan.transform.position = canTrigger.CanSpawnPoint.position;

        canTrigger.isCanPositioned = true;
        heldCan = null;

        isHoldingCan = false;
        hasPlacedCan = true;

        RpcCrossFadeAnimation("Pickup");
    }

    // ---------------- RETURN ----------------
    void ReturnToPosition()
    {
        if (returnToPosition == null) return;

        Vector3 dir = (returnToPosition.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, returnToPosition.position);

        if (dist > stoppingDistance)
        {
            characterController.Move(dir * followSpeed * Time.deltaTime);
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
            RpcCrossFadeAnimation("RunningAnimation");
        }
        else
        {
            RpcCrossFadeAnimation("idle");
        }
    }

    // ---------------- ANIMATION ----------------
    [ClientRpc]
    void RpcCrossFadeAnimation(string anim)
    {
        if (animationComponent == null) return;
        animationComponent.CrossFade(anim, crossFadeDuration);
    }

    void DebugAnimationClips()
    {
        if (animationComponent == null) return;
        foreach (AnimationState state in animationComponent)
        {
            Debug.Log("AI Animation Clip: " + state.name);
        }
    }
}

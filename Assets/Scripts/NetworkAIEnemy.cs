using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class NetworkAIEnemy : NetworkBehaviour
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
    private Transform currentTarget;

    // Run only on server
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Find all players by tag
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        targets = new Transform[playerObjects.Length];

        for (int i = 0; i < playerObjects.Length; i++)
        {
            targets[i] = playerObjects[i].transform;
        }

        characterController = GetComponent<CharacterController>();
        animationComponent = GetComponent<Animation>();

        if (animationComponent == null)
            Debug.LogError("No Animation component found on AI Enemy!");

        DebugAnimationClips();
    }

    void Update()
    {
        // Only the server controls AI logic
        if (!isServer) return;

        // Step 1: Pick up the can if needed
        if (heldCan == null && !canTrigger.isCanPositioned)
        {
            FollowCanPickup();
            return;
        }

        // Step 2: Carry can back to spawn
        if (isHoldingCan && !hasPlacedCan)
        {
            MoveToCanSpawn();
            return;
        }

        // Step 3: Return to base after placing can
        if (hasPlacedCan)
        {
            ReturnToPosition();
            return;
        }

        // Step 4: No players? Do nothing
        if (targets == null || targets.Length == 0) return;

        // Step 5: Return to base if everyone is safe
        if (AllPlayersInSafeZone())
        {
            ReturnToPosition();
            return;
        }

        // Step 6: Chase nearest player
        currentTarget = GetNearestPlayer();

        if (currentTarget != null)
        {
            if (isHoldingCan) return;

            playerPickandThrow = currentTarget.GetComponent<PickandThrow>();
            if (playerPickandThrow != null)
            {
                isTargetInSafeZone = playerPickandThrow.isOnSafeZone;
                isPickingUp = playerPickandThrow.isPickingUp;
            }

            if (!isTargetInSafeZone && isPickingUp)
                FollowPlayer();
            else
                StopFollowing();
        }
    }

    // ---------------- PLAYER & SAFE ZONE ----------------
    Transform GetNearestPlayer()
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform player in targets)
        {
            if (player == null) continue;
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = player;
            }
        }

        return nearest;
    }

    bool AllPlayersInSafeZone()
    {
        foreach (Transform player in targets)
        {
            if (player == null) continue;
            var pick = player.GetComponent<PickandThrow>();
            if (pick != null && !pick.isOnSafeZone)
                return false;
        }
        return true;
    }

    // ---------------- FOLLOW PLAYER ----------------
    void FollowPlayer()
    {
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        characterController.Move(dir * followSpeed * Time.deltaTime);

        RpcCrossFadeAnimation("RunningAnimation");
    }

    void StopFollowing()
    {
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

        RpcCrossFadeAnimation("idle");
    }

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

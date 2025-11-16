using UnityEngine;

public class AIEnemy : MonoBehaviour
{
    public CanTrigger canTrigger;
    public Transform holdPoint; // child under hand
    public GameObject heldCan = null;
    public Transform[] targets;  // Array of player transforms to choose from
    public Transform returnToPosition;  // The position the AI returns to if all players are in the safe zone
    public float followSpeed = 3.0f;  // Custom speed for the AI to follow
    public float stoppingDistance = 2.0f;  // Distance at which the AI stops following
    public float rotationSpeed = 5.0f;  // Speed at which the AI rotates to face the target
    public float crossFadeDuration = 0.2f;  // Duration for the animation crossfade

    private CharacterController characterController;
    private Animation animationComponent;  // Legacy Animation component

    private PickandThrow playerPickandThrow;
    public bool isTargetInSafeZone = false;
    public bool isPickingUp = false;
    public bool isHoldingCan = false;
    public bool hasPlacedCan = false; // Track if can is already placed
    private Transform currentTarget;
    private Vector3 velocity;      // Vertical velocity for gravity
    public float gravity = -9.81f;


    void Start()
    {
        // Find all game objects with the "Player" tag
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        // Initialize the targets array based on the found players
        targets = new Transform[playerObjects.Length];

        for (int i = 0; i < playerObjects.Length; i++)
        {
            targets[i] = playerObjects[i].transform;  // Set the transform of each player object
        }

        // Get the required components
        characterController = GetComponent<CharacterController>();
        animationComponent = GetComponent<Animation>();  // Use Legacy Animation

        if (animationComponent == null)
        {
            Debug.LogError("No Animation component found!");
        }

        DebugAnimationClips();
    }

    void Update()
    {
        ApplyGravity();
        // Step 1: If not holding the can and can is not in spawn, go pick it up
        if (heldCan == null && !canTrigger.isCanPositioned)
        {
            followCanPickup();
            return;
        }

        // Step 2: If holding the can, go to spawn point to place it
        if (isHoldingCan && !hasPlacedCan)
        {
            MoveToCanSpawn();
            return;
        }

        // Step 3: After placing the can, go back home
        if (hasPlacedCan)
        {
            isHoldingCan = false; // Reset holding state
            heldCan = null;    // Clear held can reference
            canTrigger.isCanPositioned = true; // Mark can as positioned
            hasPlacedCan = false;
            //     Debug.Log("Can has been placed back at spawn point.bbbbbb");
            ReturnToPosition();
            return;
        }

        // Step 4: If all players are in the safe zone, idle or go home
        if (targets.Length == 0) return;

        if (AllPlayersInSafeZone())
        {
            ReturnToPosition();
            return;
        }

        // Step 5: Chase logic
        currentTarget = GetNearestPlayer();

        if (currentTarget != null)
        {
            if (isHoldingCan) return; // Don’t chase if carrying the can

            playerPickandThrow = currentTarget.GetComponent<PickandThrow>();

            if (playerPickandThrow != null)
            {
                isTargetInSafeZone = playerPickandThrow.isOnSafeZone;
                isPickingUp = playerPickandThrow.isPickingUp;
            }

            if (!isTargetInSafeZone && isPickingUp)
            {
                FollowPlayer();
            }
            else
            {
                StopFollowing();
            }
        }
    }

    // ------------------ PLAYER & SAFE ZONE HANDLING ------------------

    Transform GetNearestPlayer()
    {
        Transform nearestPlayer = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform player in targets)
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPlayer = player;
                }
            }
        }
        return nearestPlayer;
    }

    bool AllPlayersInSafeZone()
    {
        foreach (Transform player in targets)
        {
            if (player != null)
            {
                var pickAndThrow = player.GetComponent<PickandThrow>();
                if (pickAndThrow != null && !pickAndThrow.isOnSafeZone)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // ------------------ FOLLOW / CHASE PLAYER ------------------

    void FollowPlayer()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;

        // Rotate toward player
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        // Move toward player
        characterController.Move(direction * followSpeed * Time.deltaTime);

        CrossFadeAnimation("RunningAnimation");
    }
    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f; // small downward push to stick to ground

        velocity.y += gravity * Time.deltaTime;
    }


    void StopFollowing()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        CrossFadeAnimation("Idle");
    }

    // ------------------ CAN PICKUP LOGIC ------------------

    void followCanPickup()
    {
        if (!isHoldingCan)
        {
            Vector3 direction = (canTrigger.Can.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

            characterController.Move(direction * followSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, canTrigger.Can.position) < stoppingDistance)
            {
                TryPickupCan(canTrigger.Can.GetComponent<Collider>());
            }

            CrossFadeAnimation("RunningAnimation");
        }
    }

    public void TryPickupCan(Collider canCollider)
    {
        if (heldCan == null)
        {
            heldCan = canCollider.gameObject;
            heldCan.transform.SetParent(holdPoint);
            heldCan.transform.localPosition = Vector3.zero;
            heldCan.transform.localRotation = Quaternion.identity;

            Rigidbody rb = heldCan.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb);
            }

            isHoldingCan = true;
            hasPlacedCan = false;

            CrossFadeAnimation("Pickup");
            Debug.Log($"Picked up {heldCan.name}");
        }
    }

    // ------------------ CAN PLACEMENT LOGIC ------------------

    void MoveToCanSpawn()
    {
        Vector3 direction = (canTrigger.CanSpawnPoint.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        characterController.Move(direction * followSpeed * Time.deltaTime);

        CrossFadeAnimation("RunningAnimation");

        // Once close enough, place can
        if (Vector3.Distance(transform.position, canTrigger.CanSpawnPoint.position) <= stoppingDistance)
        {
            PlaceCanBack();
        }
    }

    void PlaceCanBack()
    {
        if (heldCan != null)
        {
            heldCan.transform.SetParent(null);
            heldCan.transform.position = canTrigger.CanSpawnPoint.position;

            canTrigger.isCanPositioned = true;
            heldCan = null;

            isHoldingCan = false;
            hasPlacedCan = true;

            CrossFadeAnimation("Pickup");
            Debug.Log("Placed can back at spawn point");
        }
    }

    // ------------------ RETURN TO BASE ------------------

    void ReturnToPosition()
    {
        if (returnToPosition != null)
        {
            Vector3 direction = (returnToPosition.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, returnToPosition.position);

            if (distance > stoppingDistance)
            {
                characterController.Move(direction * followSpeed * Time.deltaTime);
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                CrossFadeAnimation("RunningAnimation");
            }
            else
            {
                CrossFadeAnimation("Idle");
            }
        }
    }

    // ------------------ ANIMATION HANDLING ------------------

    void CrossFadeAnimation(string animationName)
    {
        if (animationComponent != null)
        {
            animationComponent.CrossFade(animationName, crossFadeDuration);
        }
    }

    void DebugAnimationClips()
    {
        if (animationComponent != null)
        {
            foreach (AnimationState state in animationComponent)
            {
                Debug.Log("Animation Clip: " + state.name);
            }
        }
    }
}

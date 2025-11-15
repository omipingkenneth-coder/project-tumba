using UnityEngine;
using Mirror;
using System.Collections;

[RequireComponent(typeof(NetworkCharacterControllerMovement))]
public class NetworkPickAndThrow : NetworkBehaviour
{
    [Header("References")]
    public Transform holdPoint;
    public GameObject ArrowAbove;
    public LayerMask pickableLayer;
    public Transform startPointThrow;
    public GameObject SlipperPrefab;
    public bool isOnSafeZone = true;

    [Header("Settings")]
    public float pickUpRange = 3f;
    public float throwForce = 10f;
    public float pickAnimDuration = 1.0f;
    public float throwAnimDuration = 0.6f;

    [Header("State")]
    public GameObject MoveToGuide;
    public GameObject heldSlipper = null;
    public float pickupCooldown = 0.5f;
    private float lastThrowTime = -1f;

    private NetworkCharacterControllerMovement movement;

    void Start()
    {
        movement = GetComponent<NetworkCharacterControllerMovement>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Desktop input (E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldSlipper == null)
                LocalPickObject();
            else
                LocalThrowObject();
        }
    }

    // -----------------------------
    // MOBILE INPUT
    // -----------------------------
    public void MobilePickup()
    {
        if (!isLocalPlayer || heldSlipper != null) return;
        LocalPickObject();
    }

    public void MobileThrow()
    {
        if (!isLocalPlayer || heldSlipper == null) return;
        LocalThrowObject();
    }

    // -----------------------------
    // LOCAL PICK / THROW
    // -----------------------------
    void LocalPickObject()
    {
        // // Find nearest pickable object in range
        // Collider[] hits = Physics.OverlapSphere(transform.position, pickUpRange, pickableLayer);
        // if (hits.Length == 0) return;

        // // Pick first valid object
        // TryPickup(hits[0]);

        // Play local animation
        if (movement.animationComponent != null)
            movement.animationComponent.CrossFade("Pick Object", 0.1f);
        movement.currentAnim = AnimState.Pick;

        StartCoroutine(ReturnToIdleAfter(pickAnimDuration));

        CmdPickObject();
    }

    void LocalThrowObject()
    {
        if (heldSlipper == null) return;

        // Play local animation
        if (movement.animationComponent != null)
            movement.animationComponent.CrossFade("Throw Object", 0.1f);
        movement.currentAnim = AnimState.Throw;

        StartCoroutine(ReturnToIdleAfter(throwAnimDuration));

        CmdThrowObject();
    }

    IEnumerator ReturnToIdleAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        movement.currentAnim = AnimState.Idle;
        if (movement.animationComponent != null)
            movement.animationComponent.CrossFade("Idle", 0.15f);
    }

    // -----------------------------
    // HYBRID PICK
    // -----------------------------
    // Call this from local player input
    public void TryPickup(Collider other)
    {
        if (!isLocalPlayer) return;

        if (heldSlipper != null || Time.time < lastThrowTime + pickupCooldown)
        {
            Debug.LogWarning("Cannot pick up: already holding a slipper or in cooldown.");
            return;
        }

        CmdPickup(other.gameObject);
    }

    [Command]
    private void CmdPickup(GameObject slipper)
    {
        if (slipper == null) return;

        // Make kinematic on the server
        if (slipper.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        heldSlipper = slipper;
        MoveToGuide.SetActive(true);

        // Set parent on server (authoritative)
        heldSlipper.transform.SetParent(holdPoint);
        heldSlipper.transform.localPosition = Vector3.zero;
        heldSlipper.transform.localRotation = Quaternion.identity;

        // Notify clients to update parent as well
        RpcPickup(heldSlipper);
    }

    [ClientRpc]
    private void RpcPickup(GameObject slipper)
    {
        if (slipper == null) return;
        heldSlipper = slipper.gameObject;
        MoveToGuide.SetActive(true);
        slipper.transform.SetParent(holdPoint);
        slipper.transform.localPosition = Vector3.zero;
        slipper.transform.localRotation = Quaternion.identity;
    }
    [Command]
    void CmdPickObject()
    {
        if (heldSlipper == null) return;

        // Notify remote clients to play Pick
        RpcPlayAnimation(AnimState.Pick);
    }

    [Command]
    void CmdThrowObject()
    {
        if (heldSlipper == null) return;

        RpcPlayAnimation(AnimState.Throw);
        StartCoroutine(ServerThrowRoutine());
    }

    IEnumerator ServerThrowRoutine()
    {
        yield return new WaitForSeconds(throwAnimDuration);

        MoveToGuide.SetActive(false);

        // Destroy the currently held slipper (if networked)
        if (heldSlipper != null && heldSlipper.TryGetComponent<NetworkIdentity>(out var ni))
        {
            NetworkServer.Destroy(heldSlipper);
        }
        heldSlipper = null;

        // Spawn a new networked slipper
        GameObject newSlipper = Instantiate(SlipperPrefab, startPointThrow.position, startPointThrow.rotation);

        Rigidbody rb = newSlipper.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(startPointThrow.forward * throwForce, ForceMode.Impulse);

        // Spawn it on the network
        NetworkServer.Spawn(newSlipper, connectionToClient);

        lastThrowTime = Time.time;

        RpcPlayAnimation(AnimState.Idle);
    }

    // -----------------------------
    // RPC: SYNC ANIMATION
    // -----------------------------
    [ClientRpc]
    void RpcPlayAnimation(AnimState state)
    {
        if (isLocalPlayer) return; // Local player already plays animation

        if (movement == null || movement.animationComponent == null) return;

        switch (state)
        {
            case AnimState.Pick:
                movement.animationComponent.CrossFade("Pick Object", 0.1f);
                movement.currentAnim = AnimState.Pick;
                break;
            case AnimState.Throw:
                movement.animationComponent.CrossFade("Throw Object", 0.1f);
                movement.currentAnim = AnimState.Throw;
                break;
            case AnimState.Idle:
                movement.animationComponent.CrossFade("Idle", 0.15f);
                movement.currentAnim = AnimState.Idle;
                break;
        }
    }
}

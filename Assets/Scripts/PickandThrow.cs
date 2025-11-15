using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

[System.Serializable]
public class Slipper
{
    public string name;
    public GameObject slipperObject;
    public float mass = 1.0f;
    public float accuracy = 0.1f;
}

public class PickandThrow : MonoBehaviour
{
    [Header("References")]
    public GameObject SlipperPrefab;             // Reference to the slipper prefab
    public GameObject MoveToGuide;
    public CharacterControllerMovement movementScript;
    public Transform startPointThrow;
    public Transform holdPoint;                  // Child under hand
    public PlayerCameraController cameraController; // Camera reference for aim raycast

    [Header("Slipper Settings")]
    public List<Slipper> slippers = new List<Slipper>();
    public float throwForce = 20f;
    public float pickupCooldown = 0.5f;

    [Header("Input Settings")]
    public KeyCode throwKey = KeyCode.Mouse0;    // Left Mouse
    public KeyCode pickKey = KeyCode.E;          // Pickup key

    public bool isOnSafeZone = true;
    public bool isPickingUp = false;

    private float lastThrowTime = -1f;
    public GameObject heldSlipper = null;

    // -------------------- Unity Methods --------------------
    void Update()
    {
        HandleInput();
    }

    // -------------------- Input Handling --------------------
    void HandleInput()
    {
        if (heldSlipper != null && Input.GetKeyDown(throwKey))
        {
            StartCoroutine(ThrowSequence());
        }
        else if (Input.GetKeyDown(pickKey))
        {
            // Pickup if nothing is already held
            if (heldSlipper == null)
            {
                if (movementScript != null)
                    StartCoroutine(movementScript.PlayPickAnimation());
                else
                    Debug.LogWarning("movementScript reference not set in PickandThrow!");
            }
        }
    }

    // -------------------- Mobile Controls --------------------
    public void MobilePickup()
    {
        if (movementScript != null)
            StartCoroutine(movementScript.PlayPickAnimation());
        else
            Debug.LogWarning("movementScript reference not set in PickandThrow!");
    }

    public void MobileThrow()
    {
        if (heldSlipper != null)
            StartCoroutine(ThrowSequence());
    }

    // -------------------- Pickup Logic --------------------
    public void TryPickup(Collider other)
    {
        // Ensure no slipper is held and no cooldown
        if (heldSlipper != null || Time.time < lastThrowTime + pickupCooldown)
        {
            Debug.LogWarning("Cannot pick up: already holding a slipper or in cooldown.");
            return;
        }
        if (other.GetComponent<Rigidbody>() != null)
        {
            Destroy(other.GetComponent<Rigidbody>());
        }

        // Assign the slipper to heldSlipper directly
        heldSlipper = other.gameObject;
        MoveToGuide.SetActive(true);

        // Parent the slipper to the hand (holdPoint)
        heldSlipper.transform.parent = holdPoint;
        heldSlipper.transform.localPosition = Vector3.zero;
        heldSlipper.transform.localRotation = Quaternion.identity;

        isPickingUp = true;

        Debug.Log($"Picked up {heldSlipper.name}");
    }

    // -------------------- Throwing Sequence --------------------
    private IEnumerator ThrowSequence()
    {
        if (movementScript.animationComponent == null)
            yield break;

        movementScript.isThrowing = true;

        // Play throw animation
        movementScript.animationComponent.CrossFade("Throw Object", 0.1f);

        // Wait for animation length or fallback
        yield return new WaitForSeconds(0.5f);

        // Actually throw the slipper
        ThrowSlipper();

        // Small buffer before allowing movement animations again
        yield return new WaitForSeconds(0.2f);

        movementScript.isThrowing = false;
    }

    // -------------------- Throwing Mechanics --------------------
    public void ThrowSlipper()
    {
        if (heldSlipper == null)
        {
            Debug.LogError("No slipper is currently held.");
            return;
        }

        isPickingUp = false;
        MoveToGuide.SetActive(false);

        // Destroy the currently held slipper object
        Destroy(heldSlipper);
        Debug.Log("Destroyed the held slipper.");

        // Instantiate a new slipper at the throw point
        GameObject newSlipper = Instantiate(SlipperPrefab, startPointThrow.position, Quaternion.identity);
        newSlipper.GetComponent<Bullet>().Owner = gameObject; // Assign owner reference

        // Setup Rigidbody
        Rigidbody rb = newSlipper.GetComponent<Rigidbody>();
        rb.mass = 1.0f;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Calculate throw direction
        Vector3 throwDirection = startPointThrow.forward;

        // Apply throw force
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        // Apply spin torque
        Vector3 spinTorque = new Vector3(0, 10f, 0);
        rb.AddTorque(spinTorque, ForceMode.Impulse);

        Debug.Log($"Threw new slipper from {startPointThrow.position} with force {throwForce} and spin torque {spinTorque}");

        // Reset
        heldSlipper = null;
        lastThrowTime = Time.time;
    }
}

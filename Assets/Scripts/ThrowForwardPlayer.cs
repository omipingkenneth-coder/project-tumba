using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SlipperHave
{
    public string name;
    public GameObject slipperObject;
    public float mass = 1.0f;
    public float accuracy = 0.05f;
}

public class ThrowForwardPlayer : MonoBehaviour
{
    [Header("References")]
    public GameObject SlipperPrefab;
    public CharacterControllerMovement movementScript;
    public Transform startPointThrow;
    public Transform holdPoint;
    public Transform endPointThrow; // target
    public PlayerCameraController cameraController;

    [Header("Slipper Settings")]
    public List<SlipperHave> slippers = new List<SlipperHave>();
    public float throwSpeed = 20f;
    public float pickupCooldown = 0.5f;
    public float angularSpin = 5f;
    public float drag = 0.2f;
    public float angularDrag = 0.3f;

    [Header("Input Settings")]
    public KeyCode throwKey = KeyCode.Mouse0;
    public KeyCode pickKey = KeyCode.E;

    private float lastThrowTime = -1f;
    private SlipperHave heldSlipper = null;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (heldSlipper != null && Input.GetKeyDown(throwKey))
            StartCoroutine(ThrowSequence());
        else if (Input.GetKeyDown(pickKey))
            PickAnimation();
    }

    void PickAnimation()
    {
        if (movementScript != null)
            StartCoroutine(movementScript.PlayPickAnimation());
        else
            Debug.LogWarning("movementScript reference not set!");
    }

    public void TryPickup(Collider other)
    {
        if (heldSlipper != null || Time.time < lastThrowTime + pickupCooldown)
            return;

        SlipperHave slipper = slippers.FirstOrDefault(s => s.slipperObject == other.gameObject);
        if (slipper == null) return;

        heldSlipper = slipper;
        other.gameObject.transform.parent = holdPoint;
        Destroy(other.gameObject.GetComponent<Rigidbody>());
        other.gameObject.transform.localPosition = Vector3.zero;
        other.gameObject.transform.localRotation = Quaternion.identity;

        Debug.Log($"Picked up {slipper.name}");
    }

    IEnumerator ThrowSequence()
    {
        if (movementScript.animationComponent == null) yield break;

        movementScript.isThrowing = true;
        movementScript.animationComponent.CrossFade("Throw Object", 0.1f);
        yield return new WaitForSeconds(0.5f);

        ThrowSlipper();
        yield return new WaitForSeconds(0.2f);

        movementScript.isThrowing = false;
    }

    public void ThrowSlipper()
    {
        if (heldSlipper == null) return;

        GameObject slipperObject = heldSlipper.slipperObject;
        slipperObject.transform.parent = null;
        slipperObject.transform.position = startPointThrow.position;

        Vector3 targetPos = endPointThrow != null ? endPointThrow.position : transform.position + transform.forward * 10f;
        Vector3 throwDir = (targetPos - startPointThrow.position).normalized;

        // Apply small random offset for "accuracy"
        throwDir += Random.insideUnitSphere * heldSlipper.accuracy;
        throwDir.Normalize();

        slipperObject.transform.rotation = Quaternion.LookRotation(throwDir, Vector3.up);

        Rigidbody rb = slipperObject.GetComponent<Rigidbody>();
        if (rb == null) rb = slipperObject.AddComponent<Rigidbody>();

        rb.mass = heldSlipper.mass;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Set initial velocity directly once
        rb.linearVelocity = throwDir * throwSpeed;

        // Add spin
        rb.angularVelocity = Random.insideUnitSphere * angularSpin;

        heldSlipper = null;
        lastThrowTime = Time.time;

        Debug.Log($"Threw slipper toward {(endPointThrow != null ? endPointThrow.name : "forward")}");
    }
}

using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody), typeof(MeshCollider))]
public class NetworkCanPhysics : NetworkBehaviour
{
    [Header("Can Physics Settings")]
    public float mass = 0.2f;
    public float drag = 0.5f;
    public float angularDrag = 1f;
    public float bounceFactor = 0.1f;

    private Rigidbody rb;
    private MeshCollider meshCollider;
    private bool hasCollided = false;

    // Networked state sync
    [SyncVar] private Vector3 syncPosition;
    [SyncVar] private Quaternion syncRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshCollider = GetComponent<MeshCollider>();

        // Configure rigidbody
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Configure collider
        meshCollider.convex = true;
        PhysicsMaterial mat = new PhysicsMaterial
        {
            bounciness = bounceFactor,
            dynamicFriction = 1f,
            staticFriction = 1f,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };
        meshCollider.material = mat;
    }

    void FixedUpdate()
    {
        // Only server drives the physics simulation
        if (isServer)
        {
            syncPosition = rb.position;
            syncRotation = rb.rotation;
        }
        else
        {
            // Clients interpolate to smooth out network motion
            rb.position = Vector3.Lerp(rb.position, syncPosition, Time.deltaTime * 10);
            rb.rotation = Quaternion.Lerp(rb.rotation, syncRotation, Time.deltaTime * 10);
        }
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        // Handle small bounce or hit logic only on server
        if (!hasCollided)
        {
            // Add a small impulse for feedback
            rb.AddForce(Vector3.up * 0.5f, ForceMode.Impulse);
            hasCollided = true;
        }

        // Reset after delay to allow re-collision effects
        Invoke(nameof(ResetCollisionFlag), 0.3f);
    }

    [Server]
    private void ResetCollisionFlag()
    {
        hasCollided = false;
    }
}

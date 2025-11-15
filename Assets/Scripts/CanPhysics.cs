using UnityEngine;

public class CanPhysics : MonoBehaviour
{
    public float mass = 0.2f;           // Light mass (similar to a can)
    public float drag = 0.5f;           // Low drag for easy movement
    public float angularDrag = 1f;      // Low angular drag to allow rolling
    public float bounceFactor = 0.1f;   // Small bounce (bounciness)
    private Rigidbody rb;
    private MeshCollider meshCollider;
    private bool hasCollided = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();  // Add Rigidbody if not present
        }

        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;

        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        meshCollider.convex = true;  // Set convex for dynamic collisions

        // Set the initial Physic Material with a low bounce factor
        PhysicsMaterial material = new PhysicsMaterial();
        material.bounciness = bounceFactor;  // Initial bounciness
        material.dynamicFriction = 1f;    // Moderate dynamic friction
        material.staticFriction = 1f;     // Moderate static friction
        meshCollider.material = material;
    }

    void OnCollisionEnter(Collision collision)
    {
        // if (!hasCollided)
        // {
        //     // Reduce bounciness after the first collision
        //     PhysicsMaterial material = new PhysicsMaterial();
        //     material.bounciness = 0f;    // No bounce after the first collision
        //     material.dynamicFriction = 0.5f;
        //     material.staticFriction = 0.5f;
        //     meshCollider.material = material;  // Apply new Physic Material

        //     hasCollided = true;
        // }

        // Apply a small force if necessary
        rb.AddForce(Vector3.up * 0.5f, ForceMode.Impulse);
    }


}

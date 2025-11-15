using UnityEngine;

public class ThrowObject : MonoBehaviour
{
    public string throwPointTag = "StartThrowPoint"; // Tag for throw point object
    public Vector3 customDirection = Vector3.forward; // The default direction for the throw
    public float throwForce = 10f; // The force applied to the object
    public float throwAccuracy = 1f; // A value to adjust the accuracy of the throw (1 is perfect accuracy)

    private Rigidbody rb; // The rigidbody component of the object
    private Transform throwPoint; // The point from where the object will be thrown

    void Awake()
    {
        // Find the GameObject with the specified tag and assign its Transform
        GameObject throwPointObject = GameObject.FindGameObjectWithTag(throwPointTag);

        if (throwPointObject != null)
        {
            throwPoint = throwPointObject.transform;
        }
        else
        {
            Debug.LogWarning("Throw point with tag " + throwPointTag + " not found!");
        }

        // Get the Rigidbody component of the object this script is attached to
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on this GameObject. Please add a Rigidbody.");
        }
    }

    void Start()
    {
        // Optionally, you can check if the throwPoint is valid here, 
        // but it's already validated in Awake().
        if (throwPoint == null)
        {
            Debug.LogWarning("Throw point is not assigned or found. Ensure you have a valid throw point.");
        }
    }

    void Update()
    {
        // Check for user input to throw the object (e.g., left mouse button or Ctrl)
        if (Input.GetButtonDown("Fire1"))
        {
            Throw();
        }
    }

    public void Throw()
    {
        // Ensure the object has a Rigidbody component and a valid throwPoint
        if (rb != null && throwPoint != null)
        {
            // Calculate the direction to throw towards (using the throwPoint position)
            Vector3 throwDirection = (throwPoint.position - transform.position).normalized;

            // Optionally, adjust the direction slightly for accuracy (randomize by the "throwAccuracy" factor)
            throwDirection.x += Random.Range(-throwAccuracy, throwAccuracy);
            throwDirection.y += Random.Range(-throwAccuracy, throwAccuracy);
            throwDirection.z += Random.Range(-throwAccuracy, throwAccuracy);

            // Normalize the direction again in case accuracy adjustments made the vector non-unit length
            throwDirection.Normalize();

            // Reset the Rigidbody's velocity and apply the throw force in the calculated direction
            rb.linearVelocity = Vector3.zero;  // Reset any previous velocity
            rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning("Throw failed: Rigidbody or throwPoint is missing.");
        }
    }
}

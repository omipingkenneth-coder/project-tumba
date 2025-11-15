using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f; // Bullet speed

    public Rigidbody rb; // Reference to the Rigidbody
    public GameObject Owner;
    public Bullet bulletScript;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component on the bullet
        rb.linearVelocity = transform.forward * speed; // Apply velocity in the forward direction
        //Destroy(gameObject, lifeTime); // Destroy the bullet after a certain time
    }

    // Detect collision with other objects
    private void OnCollisionEnter(Collision collision)
    {
        // You can add logic here to check what the bullet hits
        Debug.Log("Bullet hit: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Can"))
        {
            bulletScript = GetComponent<Bullet>();
            bulletScript.Owner.GetComponent<PlayerProperties>().UpdateScore();
            bulletScript.Owner.GetComponent<PlayerProperties>().UpdateExp();
        }

    }
}

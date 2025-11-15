using UnityEngine;
using Mirror;

public class NetworkBullet : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;          // Bullet speed
    public Rigidbody rb;               // Rigidbody reference
    [SyncVar] public GameObject Owner; // Who fired the bullet

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Ensure Rigidbody exists
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Give it forward motion on the server for authoritative tracking
        //  rb.linearVelocity = transform.forward * speed;

        // Auto destroy after lifeTime seconds
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

        Debug.Log($"[Server] Bullet hit: {collision.gameObject.name}");

        // Only award points for objects tagged "Can"
        if (collision.gameObject.CompareTag("Can") && Owner != null)
        {
            NetworkPlayerProperties playerProps = Owner.GetComponent<NetworkPlayerProperties>();
            if (playerProps != null)
            {
                playerProps.CmdUpdateScore();
                playerProps.CmdUpdateExp();
            }

        }
    }


}

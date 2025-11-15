using UnityEngine;

public class HandPickupCan : MonoBehaviour
{
    public AIEnemy aIEnemy;

    [Header("Mode Settings")]
    public bool isMultiplayer = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Can")) return;

        Debug.Log($"Hand collided with {other.name}");

        if (isMultiplayer)
        {
            HandleMultiplayerPickup(other);
        }
        else
        {
            HandleOfflinePickup(other);
        }
    }

    void HandleMultiplayerPickup(Collider other)
    {
        Debug.Log("Multiplayer Pickup");

        // In multiplayer, you should tell the server to handle pickup
        // Example: Send a Command if this is a NetworkBehaviour
        // aIEnemy.CmdPickupCan(other.gameObject);

        // For now, just remove Rigidbody locally (not network-safe)
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
            Destroy(rb);
    }

    void HandleOfflinePickup(Collider other)
    {
        Debug.Log("Offline Pickup");

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
            Destroy(rb);

        aIEnemy.TryPickupCan(other);
    }
}

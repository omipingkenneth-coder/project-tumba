using Mirror.BouncyCastle.Crypto.Modes;
using UnityEngine;

public class HandPickupTrigger : MonoBehaviour
{
    [Header("Mode Settings")]
    [Tooltip("Enable if this is used in a Mirror multiplayer setup.")]
    public bool isMultiplayer = false;

    [Header("References")]
    [Tooltip("Local single-player Pick and Throw script.")]
    public PickandThrow pickAndThrow;

    [Tooltip("Network-based Pick and Throw script for multiplayer.")]
    public NetworkPickAndThrow networkPickAndThrow;


    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        // Only react to objects tagged as "Slipper"
        if (!other.CompareTag("Slipper"))
            return;
        // rb.isKinematic = true;
        Destroy(rb);
        // Handle pickup based on mode
        if (isMultiplayer)
        {
            if (networkPickAndThrow != null)
            {
                networkPickAndThrow.TryPickup(other);
            }
            else
            {
                Debug.LogWarning("NetworkPickAndThrow reference not assigned for multiplayer mode!");
            }
        }
        else
        {
            if (pickAndThrow != null)
            {

                pickAndThrow.TryPickup(other);
            }
            else
            {
                Debug.LogWarning("PickandThrow reference not assigned for single-player mode!");
            }
        }
    }
}

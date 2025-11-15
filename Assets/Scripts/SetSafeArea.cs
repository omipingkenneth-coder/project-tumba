using UnityEngine;

public class SetSafeArea : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get the PickandThrow script instead of CharacterControllerMovement
            PickandThrow PickandThrow = other.GetComponent<PickandThrow>();
            if (PickandThrow != null)
            {
                // Set the player as being in the safe zone
                PickandThrow.isOnSafeZone = true;
                Debug.Log("Player entered safe zone.");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get the PickandThrow script instead of CharacterControllerMovement
            PickandThrow PickandThrow = other.GetComponent<PickandThrow>();
            if (PickandThrow != null)
            {
                // Set the player as leaving the safe zone
                PickandThrow.isOnSafeZone = false;
                Debug.Log("Player exited safe zone.");
            }
        }
    }
}

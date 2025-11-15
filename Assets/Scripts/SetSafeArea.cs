using UnityEngine;

public class SetSafeArea : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    public bool isMultiplayer = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isMultiplayer)
            {
                var pick = other.GetComponent<NetworkPickAndThrow>();
                if (pick != null)
                {
                    pick.isOnSafeZone = true;
                    Debug.Log("Multiplayer player entered safe zone.");
                }
            }
            else
            {
                var pick = other.GetComponent<PickandThrow>();
                if (pick != null)
                {
                    pick.isOnSafeZone = true;
                    Debug.Log("Singleplayer player entered safe zone.");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isMultiplayer)
            {
                var pick = other.GetComponent<NetworkPickAndThrow>();
                if (pick != null)
                {
                    pick.isOnSafeZone = false;
                    Debug.Log("Multiplayer player exited safe zone.");
                }
            }
            else
            {
                var pick = other.GetComponent<PickandThrow>();
                if (pick != null)
                {
                    pick.isOnSafeZone = false;
                    Debug.Log("Singleplayer player exited safe zone.");
                }
            }
        }
    }
}

using UnityEngine;
using System.Collections;
using Mirror;
using Unity.VisualScripting;

public class CanTrigger : NetworkBehaviour
{
    [Header("Game Mode")]
    public bool isMultiplayer = false;   // ✔ Toggle this in inspector

    [Header("References")]
    public Transform CanSpawnPoint;
    public GameObject CanPrefab;
    public Transform Can;
    public GameObject ArrowObj;

    [Header("State")]
    public bool isCanPositioned = false;
    private bool isReplacing = false;
    GameObject newCan;
    void Start()
    {
        if (!isServer) return;
        newCan = Instantiate(CanPrefab, CanSpawnPoint.position, CanSpawnPoint.rotation);
        NetworkServer.Spawn(newCan);  // ✔ Multiplayer spawn
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isReplacing) return;

        // In multiplayer, only the SERVER should replace cans
        if (isMultiplayer && !NetworkServer.active) return;

        if (other.CompareTag("Can"))
        {
            ArrowObj.SetActive(true);
            StartCoroutine(ReplaceCanRoutine(other.gameObject));
        }
    }

    private IEnumerator ReplaceCanRoutine(GameObject oldCan)
    {
        isReplacing = true;

        yield return new WaitForSeconds(0.1f);

        if (oldCan != null)
        {
            if (isMultiplayer)
                NetworkServer.Destroy(oldCan);
            else
                Destroy(oldCan);
        }



        if (isMultiplayer)
        {
            newCan = Instantiate(CanPrefab, CanSpawnPoint.position, CanSpawnPoint.rotation);
            NetworkServer.Spawn(newCan);  // ✔ Multiplayer spawn
        }
        else
        {
            newCan = Instantiate(CanPrefab, CanSpawnPoint.position, CanSpawnPoint.rotation);
        }

        newCan.tag = "Can";

        Can = newCan.transform;
        isCanPositioned = true;

        yield return new WaitForSeconds(0.5f);
        isReplacing = false;
    }
}

using Mirror;
using UnityEngine;

public class SpawnOnStart : NetworkBehaviour
{
    public GameObject prefabToSpawn;

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameObject obj = Instantiate(prefabToSpawn, transform.position, transform.rotation);
        NetworkServer.Spawn(obj);
    }
}

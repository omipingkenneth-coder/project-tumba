using UnityEngine;
using Mirror;

public class HideLocalObject : NetworkBehaviour
{
    public GameObject target; // object you want to hide locally
    public GameObject ArrowGuideTargetObject;
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        target.SetActive(false); // hide only on local player
    }


    void Update()
    {
        if (!isLocalPlayer)
        {
            // Disable for all remote players
            if (!ArrowGuideTargetObject) return;
            ArrowGuideTargetObject.SetActive(false);
            return;
        }
    }
}

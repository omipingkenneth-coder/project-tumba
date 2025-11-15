using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkPlayerListSync : NetworkBehaviour
{
    public Text playerListText;

    public static NetworkPlayerListSync Instance;

    void Awake()
    {
        Instance = this;
    }

    // Called by the server to update everyone’s list
    [ClientRpc]
    public void RpcUpdatePlayerList(string playerNames)
    {
        if (playerListText != null)
        {
            playerListText.text = playerNames;
        }
    }
}

using Mirror;
using UnityEngine;

public class NewNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab);
        var sr = player.GetComponent<SendReceive>();
        sr.playerNumber = numPlayers + 1;
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}

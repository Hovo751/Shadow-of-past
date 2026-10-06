using Mirror;
using UnityEngine;

public class MirrorNetworkManager : NetworkManager
{
    public MirrorBridge bridge;
    int playersCount = 0;
    PlayerSync[] players = new PlayerSync[2];

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        playersCount++;

        base.OnServerAddPlayer(conn);

        players[playersCount - 1] = conn.identity.GetComponent<PlayerSync>();
        players[playersCount - 1].id = playersCount;
        if (playersCount == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                players[i].started = true;
            }
        }
    }
}

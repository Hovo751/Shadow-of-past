using Mirror;
using UnityEngine;

public class MirrorNetworkManager : NetworkManager
{
    public MirrorBridge bridge;
    private int playersCount = 0;
    PlayerSync[] players = new PlayerSync[2];
    public int GetPlayerCount() { 
        return playersCount; 
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("AAAAAAAAAAAAA");
        playersCount--;
        bridge.ResetFrames();
        if (playersCount == 1)
        {
            if (players[0] == null)
            {
                players[0] = players[1];
                players[0].id = 1;
                players[1] = null;
            }
            players[0].started = false;
            bridge.started = false;
        }
    }
    public override void OnStopServer()
    {
        bridge.ResetFrames();
        bridge.started = false;
        base.OnStopServer();
    }
    public override void OnStopClient()
    {
        bridge.ResetFrames();
        bridge.started = false;
        base.OnStopServer();
    }
    public override void OnStopHost()
    {
        bridge.ResetFrames();
        bridge.started = false;
        base.OnStopServer();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        playersCount++;

        base.OnServerAddPlayer(conn);

        if (playersCount < 3)
        {
            players[playersCount - 1] = conn.identity.GetComponent<PlayerSync>();
            if (playersCount == 2)
            {
                players[0].id = 1;
                players[1].id = 2;
                for (int i = 0; i < 2; i++)
                {
                    players[i].started = true;
                }
                bridge.started = true;
            }
        }
    }
}

using UnityEngine;
using Mirror;
using TMPro;

public class NetworkingUI : MonoBehaviour
{
    public MirrorNetworkManager networkManager;
    public MirrorBridge bridge;
    public TMP_InputField roomConnectId;

    public void StartServer()
    {
        NetworkManager.singleton.StartServer();
    }
    public void StartHost()
    {
        NetworkManager.singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.singleton.StartClient();
    }

    public void CreateRoom()
    {
        bridge.CreateRoom(bridge.player.id);
    }
    public void JoinRoom()
    {
        if (long.TryParse(roomConnectId.text, out long result))
            bridge.JoinRoom(bridge.player.id, result);
    }
}

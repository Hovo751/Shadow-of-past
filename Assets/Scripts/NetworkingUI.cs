using UnityEngine;
using Mirror;

public class NetworkingUI : MonoBehaviour
{
    public MirrorNetworkManager networkManager;

    public void StartServer()
    {
        NetworkManager.singleton.StartServer();
    }
    public void HostServer()
    {
        NetworkManager.singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.singleton.StartClient();
    }
}

using Mirror;

public class PlayerSync : NetworkBehaviour
{
    [SyncVar]
    public int id;
    [SyncVar]
    public bool started = false;

    private MirrorNetworkManager networkManager;
    private MirrorBridge bridge;
    private void Start()
    {
        networkManager = FindAnyObjectByType<MirrorNetworkManager>();
        bridge = FindAnyObjectByType<MirrorBridge>();
    }
    private void Update()
    {
        if (isLocalPlayer && bridge != null)
        {
            bridge.player = this;
            bridge.id = id;
            bridge.started = started;
        }
    }
}

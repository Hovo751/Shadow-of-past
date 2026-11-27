using Mirror;

public class PlayerSync : NetworkBehaviour
{
    [SyncVar]
    public long id;
    public int playerNumber;
    public bool started = false;
    public long roomId = -1;
    public long frameDifference = -1;

    private MirrorNetworkManager networkManager;
    private MirrorBridge bridge;
    private void Start()
    {
        networkManager = FindAnyObjectByType<MirrorNetworkManager>();
        bridge = FindAnyObjectByType<MirrorBridge>();
    }
    private void Update()
    {
        if (isServer) UpdateValues(id, started, roomId, frameDifference, playerNumber);
        if (isLocalPlayer && bridge != null)
        {
            bridge.player = this;
            bridge.playerNumber = playerNumber;
            bridge.roomId = roomId;
            bridge.frameDifference = frameDifference;
            bridge.started = started;
        }
    }

    [ClientRpc]
    public void UpdateValues(long _id, bool _started, long _roomId, long _frameDifference, int _playerNumber)
    {
        if (id == _id) {
            started = _started;
            frameDifference = _frameDifference;
            playerNumber = _playerNumber;
            roomId = _roomId;
        }
    }
}

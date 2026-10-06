using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using static Simulation;

public class PlayerSync : NetworkBehaviour
{
    [SyncVar]
    public int id;
    [SyncVar]
    public bool started = false;
    private void Update()
    {
        if (isLocalPlayer && FindAnyObjectByType<MirrorBridge>() != null)
        {
            FindAnyObjectByType<MirrorBridge>().id = id;
            FindAnyObjectByType<MirrorBridge>().started = started;
        }
    }
}

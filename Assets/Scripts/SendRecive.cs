using UnityEngine;
using Mirror;

public class SendRecive : NetworkBehaviour
{
    public GameSimulation sim;

    private void Update()
    {
        InputFrame input = new InputFrame
        {
            frame = sim.frame,
            moveDir = sim.p1Input
        };
    }

    [Command]
    void CmdSendInput(InputFrame input)
    {
        RpcReceiveInput(input, netId);
    }

    [ClientRpc]
    void RpcReceiveInput(InputFrame input, uint senderId)
    {
        if (netId != senderId)
        {
            sim.p1Input = input.moveDir;
        }
    }
}

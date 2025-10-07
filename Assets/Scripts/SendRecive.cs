using UnityEngine;
using Mirror;

public class SendReceive : NetworkBehaviour
{
    GameSimulation sim;

    void Start()
    {
        sim = FindObjectOfType<GameSimulation>();
    }

    void Update()
    {
        if (!isLocalPlayer) return; // only local player sends input

        int input = Input.anyKey ? 6 : 0;
        if (sim == null) sim = FindObjectOfType<GameSimulation>();
        sim.p1Input = input;
        CmdSendInput(input);
    }

    [Command]
    void CmdSendInput(int input)
    {
        RpcSetP1Input(input);
    }


    [ClientRpc(includeOwner = false)]
    void RpcSetP1Input(int input)
    {
        if (sim == null) sim = FindObjectOfType<GameSimulation>();
        sim.p2Input = input;
    }
}

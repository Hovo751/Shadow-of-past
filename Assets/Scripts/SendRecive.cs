using Mirror;
using UnityEngine;



public class SendReceive : NetworkBehaviour
{
    private GameSimulation sim;
    private InputBuffer inputBuffer;
    [SyncVar] public int playerNumber;
    [System.Serializable]
    public struct InputFrame
    {
        public int frame;
        public int moveDir;
    }
    void Start()
    {
        sim = FindObjectOfType<GameSimulation>();
        inputBuffer = FindObjectOfType<InputBuffer>();
        if (sim != null) sim.frame = 0;
        if (inputBuffer) inputBuffer.p1inputs.Reset();
        if (inputBuffer) inputBuffer.p2inputs.Reset();
    }

    void Update()
    {
        if (!isLocalPlayer) return; // only local player sends input

        InputFrame input = new InputFrame
        {
            frame = sim.frame,
            moveDir = Input.GetKey(KeyCode.LeftArrow) ? 4 : (Input.GetKey(KeyCode.RightArrow) ? 6 : 0)
        };

        if (sim == null) sim = FindObjectOfType<GameSimulation>();

        if (playerNumber == 1)
            inputBuffer.p1inputs.Set(input, false);
        else
            inputBuffer.p2inputs.Set(input, false);

        CmdSendInput(input);
    }

    [Command]
    void CmdSendInput(InputFrame input)
    {
        RpcSetInput(input);
    }

    [ClientRpc(includeOwner = false)]
    void RpcSetInput(InputFrame input)
    {
        if (sim == null) sim = FindObjectOfType<GameSimulation>();
        if (inputBuffer == null) inputBuffer = FindObjectOfType<InputBuffer>();

        if (playerNumber == 1)
            inputBuffer.p1inputs.Set(input, true);
        else
            inputBuffer.p2inputs.Set(input, true);

    }
}

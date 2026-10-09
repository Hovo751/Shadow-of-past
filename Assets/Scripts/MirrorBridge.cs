using Mirror;
using Mirror.Examples.BilliardsPredicted;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static Simulation;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class MirrorBridge : NetworkBehaviour
{
    public InputActionAsset InputActions;
    public Simulation simulation;
    public Player player1;
    public Player player2;
    public Characters characters;
    private InputAction player1Move;
    private InputAction player2Move;

    private InputAction player1L;
    private InputAction player2L;

    private InputAction player1M;
    private InputAction player2M;

    private InputAction player1H;
    private InputAction player2H;
    Dictionary<long, MovementAndButtonInput> player1Inputs = new();
    Dictionary<long, MovementAndButtonInput> player2Inputs = new();
    public long frame = 0;
    public long currentFrame = 0;
    public long serverFrame = 0;
    public int id = 0;
    public bool started = false;
    public TextMeshProUGUI t1;
    public TextMeshProUGUI t2;
    public TextMeshProUGUI t3;

    private void Start()
    {
        if (simulation != null)
        {
            simulation.SetStartParams(0, characters.characters[player1.character], characters.characters[player2.character]);
        }

        var p1 = InputActions.FindActionMap("Player1");
        var p2 = InputActions.FindActionMap("Player2");

        if (p1 == null)
        {
            Debug.LogError("Player1 action map not found!");
            return;
        }

        if (p2 == null)
        {
            Debug.LogError("Player2 action map not found!");
            return;
        }

        p1.Enable();
        p2.Enable();

        player1Move = p1.FindAction("Move");
        player1L = p1.FindAction("Light");
        player1M = p1.FindAction("Medium");
        player1H = p1.FindAction("Heavy");

        player2Move = p2.FindAction("Move");
        player2L = p2.FindAction("Light");
        player2M = p2.FindAction("Medium");
        player2H = p2.FindAction("Heavy");
    }

    private MovementAndButtonInput GetPlayerInput()
    {
        MovementAndButtonInput input = new MovementAndButtonInput();
        input.movement = -1;
        if (player1Move == null)
            return input;
        if (player1L == null)
            return input;
        if (player1M == null)
            return input;
        if (player1H == null)
            return input;
        Vector2 vec = player1Move.ReadValue<Vector2>();
        int m = 5;
        if (vec.y > 0) m += 3;
        else if (vec.y < 0) m -= 3;
        if (vec.x > 0) m++;
        else if (vec.x < 0) m--;

        input.movement = m;
        input.light = player1L.IsPressed();
        input.medium = player1M.IsPressed();
        input.heavy = player1H.IsPressed();

        return input;
    }

    MovementAndButtonInput GetPlayerInput(long _frame, int player)
    {
        MovementAndButtonInput input = new MovementAndButtonInput();
        input.movement = 5;
        input.light = false;
        input.medium = false;
        input.heavy = false;

        if (_frame < 0) return input;
        if (player == 1)
        {
            if (!player1Inputs.TryGetValue(_frame, out input))
            {
                input = GetPlayerInput(_frame - 1, player);
            }
        }
        else if (player == 2)
        {
            if (!player2Inputs.TryGetValue(_frame, out input))
            {
                input = GetPlayerInput(_frame - 1, player);
            }
        }

        return input;
    }
    private void FixedUpdate()
    {
        if (!started) return;
        if (isServer)
        {
            serverFrame++;
            SendFrame(serverFrame);
        }
        if (isClient)
        {
            if (frame < currentFrame)
            {
                MovementAndButtonInput input = GetPlayerInput();
                if (id == 1) player1Inputs[currentFrame + 1] = input;
                if (id == 2) player2Inputs[currentFrame + 1] = input;
                SendInput(input, currentFrame + 1, id);
                while (frame < currentFrame)
                {
                    MovementAndButtonInput[] player1InputsArr = new MovementAndButtonInput[64];
                    MovementAndButtonInput[] player2InputsArr = new MovementAndButtonInput[64];
                    for (int i = 0; i < 64; i++)
                    {
                        player1InputsArr[i] = GetPlayerInput(frame - i, 1);
                    }

                    for (int i = 0; i < 64; i++)
                    {
                        player2InputsArr[i] = GetPlayerInput(frame - i, 2);
                    }
                    SimulationState state = simulation.Simulate(player1InputsArr, player2InputsArr, frame);
                    player1.changebleStats = state.Player1Data;
                    player2.changebleStats = state.Player2Data;
                    frame++;
                }
            }
            if (currentFrame < serverFrame)
            {
                currentFrame++;
            }
            if (currentFrame < serverFrame - 1)
            {
                MovementAndButtonInput input = GetPlayerInput();
                if (id == 1) player1Inputs[currentFrame + 1] = input;
                if (id == 2) player2Inputs[currentFrame + 1] = input;
                SendInput(input, currentFrame + 1, id);
                while (frame < currentFrame)
                {
                    MovementAndButtonInput[] player1InputsArr = new MovementAndButtonInput[64];
                    MovementAndButtonInput[] player2InputsArr = new MovementAndButtonInput[64];
                    for (int i = 0; i < 64; i++)
                    {
                        player1InputsArr[i] = GetPlayerInput(frame - i, 1);
                    }

                    for (int i = 0; i < 64; i++)
                    {
                        player2InputsArr[i] = GetPlayerInput(frame - i, 2);
                    }
                    SimulationState state = simulation.Simulate(player1InputsArr, player2InputsArr, frame);
                    player1.changebleStats = state.Player1Data;
                    player2.changebleStats = state.Player2Data;
                    frame++;
                }
                currentFrame++;
            }
        }
    }

    private void Update()
    {
        t1.text = frame.ToString();
        t2.text = currentFrame.ToString();
        t3.text = serverFrame.ToString();
    }
    [ClientRpc]
    void SendFrame(long _frame)
    {
        started = true;
        if (!isServer)
            serverFrame = _frame;
    }
    [ClientRpc]
    void BroadcastInput(MovementAndButtonInput input, long inputFrame, int owner)
    {
        if (id != owner)
        {
            MovementAndButtonInput oldInput = GetPlayerInput(inputFrame, owner);
            if (owner == 1) player1Inputs[inputFrame] = input;
            if (owner == 2) player2Inputs[inputFrame] = input;
            if (oldInput == input) return;
            if (frame >= inputFrame)
            {
                Debug.Log("Rollback");
                frame = inputFrame;
            }
        }
    }
    [Command(requiresAuthority = false)]
    void SendInput(MovementAndButtonInput input, long inputFrame, int owner)
    {
        BroadcastInput(input, inputFrame, owner);
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using static Simulation;

public class MainLocalScript : MonoBehaviour
{
    public Simulation simulation;
    public Player player1;
    public Player player2;
    public Characters characters;
    public InputActionAsset InputActions;

    private InputAction player1Move;
    private InputAction player2Move;

    private InputAction player1L;
    private InputAction player2L;

    private InputAction player1M;
    private InputAction player2M;

    private InputAction player1H;
    private InputAction player2H;

    private bool _enabled = false;
    private long simulationFrame = 1;
    MovementAndButtonInput[] player1Inputs;
    MovementAndButtonInput[] player2Inputs;

    private void Start()
    {
        if (simulation != null)
        {
            simulation.SetStartParams(simulationFrame, characters.characters[player1.character], characters.characters[player2.character]);
        }
        player1Inputs = new MovementAndButtonInput[64];
        player2Inputs = new MovementAndButtonInput[64];
        MovementAndButtonInput noInputs = new MovementAndButtonInput();
        noInputs.movement = 5;
        noInputs.light = false;
        noInputs.medium = false;
        noInputs.heavy = false;

        for (int i = 0; i < player1Inputs.Length; i++)
            player1Inputs[i] = noInputs;
        for (int i = 0; i < player2Inputs.Length; i++)
            player2Inputs[i] = noInputs;
    }

    private void Update()
    {
        if (!_enabled)
        {
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
            _enabled = true;
        }
        MovementAndButtonInput player1Input = GetPlayer1Input();
        MovementAndButtonInput player2Input = GetPlayer2Input();
        
        for (int i = player1Inputs.Length - 2; i >= 0; i--)
        {
            player1Inputs[i + 1] = player1Inputs[i];
        }
        player1Inputs[0] = player1Input;

        for (int i = player2Inputs.Length - 2; i >= 0; i--)
        {
            player2Inputs[i + 1] = player2Inputs[i];
        }
        player2Inputs[0] = player2Input;

        SimulationState state = simulation.Simulate(player1Inputs, player2Inputs, simulationFrame);
        player1.changebleStats = state.PlayerData[0];
        player2.changebleStats = state.PlayerData[1];
        simulationFrame++;
    }

    private MovementAndButtonInput GetPlayer1Input()
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

    private MovementAndButtonInput GetPlayer2Input()
    {
        MovementAndButtonInput input = new MovementAndButtonInput();
        input.movement = -1;
        if (player2Move == null)
            return input;
        if (player2L == null)
            return input;
        if (player2M == null)
            return input;
        if (player2H == null)
            return input;
        Vector2 vec = player2Move.ReadValue<Vector2>();
        int m = 5;
        if (vec.y > 0) m += 3;
        else if (vec.y < 0) m -= 3;
        if (vec.x > 0) m++;
        else if (vec.x < 0) m--;

        input.movement = m;
        input.light = player2L.IsPressed();
        input.medium = player2M.IsPressed();
        input.heavy = player2H.IsPressed();

        return input;
    }
}
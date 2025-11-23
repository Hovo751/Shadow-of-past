using Coherence.Toolkit;
using UnityEngine;

public class Simulation : CoherenceInputSimulation<SimulationState>
{
    public Camera _camera;
    protected override void SetInputs(CoherenceClientConnection client)
    {
        var player = client.GameObject.GetComponent<Player>();
        player.SetMovement();
    }

    private bool isInAir(Player player)
    {
        if (player.verticalPos <= 0)
        {
            player.verticalPos = 0;
            player.verticalSpeed = 0;
            return false;
        }
        return true;
    }
    private void Jump(Player player, int horizontalDir)
    {
        player.verticalSpeed = player.JumpPower;
        player.horizontalSpeed = player.JumpPowerSide * horizontalDir;
    }

    protected override void Simulate(long simulationFrame)
    {
        foreach (CoherenceClientConnection client in AllClients)
        {
            Player player = client.GameObject.GetComponent<Player>();
            int movement = (int)player.GetMovement(simulationFrame);
            if (movement > 9 || movement < 1)
            {
                continue;
            }
            int horizontalMovement = (movement - 1) % 3 - 1;
            int verticalMovement = (movement - 1) / 3 - 1;
            if (isInAir(player))
            {
                player.verticalSpeed -= player.Gravity;
            }
            else
            {
                player.horizontalSpeed = player.Speed * horizontalMovement;
                if (verticalMovement == 1)
                {
                    Jump(player, horizontalMovement);
                }
            }

            player.horizontalPos += player.horizontalSpeed;
            player.verticalPos += player.verticalSpeed;
        }
    }

    protected override void Rollback(long toFrame, SimulationState state)
    {
        Debug.Log("Rollback");
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            player.horizontalPos = state.PlayerPositionsHorizontal[i];
            player.verticalPos = state.PlayerPositionsVertical[i];
        }
    }

    protected override SimulationState CreateState()
    {
        var simulationState = new SimulationState { PlayerPositionsHorizontal = new int[AllClients.Count] , PlayerPositionsVertical = new int[AllClients.Count] };
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            simulationState.PlayerPositionsHorizontal[i] = (int)(player.horizontalPos);
            simulationState.PlayerPositionsVertical[i] = (int)(player.verticalPos);
        }

        return simulationState;
    }

    protected override void OnClientJoined(CoherenceClientConnection client)
    {
        Player player1 = AllClients[0].GameObject.GetComponent<Player>();
        player1.horizontalPos = -5000;
        player1.verticalPos = 0;
        SimulationEnabled = AllClients.Count >= 2;
        if (SimulationEnabled)
        {
            Player player2 = AllClients[1].GameObject.GetComponent<Player>();
            player2.horizontalPos = 5000;
            player2.verticalPos = 0;
            _camera.player1 = player1.transform;
            _camera.player2 = player2.transform;
            StateStore.Clear();
        }
    }

    protected override void OnClientLeft(CoherenceClientConnection client)
    {
        SimulationEnabled = AllClients.Count >= 2;
    }
}
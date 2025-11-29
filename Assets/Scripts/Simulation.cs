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
            player.verticalVelocity = 0;
            return false;
        }
        return true;
    }
    private void Jump(Player player, int horizontalDir)
    {
        player.verticalVelocity = player.JumpPower;
        player.horizontalVelocity = player.JumpPowerSide * horizontalDir;
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
                player.verticalVelocity -= player.Gravity;
            }
            else
            {
                player.horizontalVelocity = player.Speed * horizontalMovement;
                if (verticalMovement == 1)
                {
                    Jump(player, horizontalMovement);
                }
            }

            player.horizontalPos += player.horizontalVelocity;
            player.verticalPos += player.verticalVelocity;

            if (player.horizontalPos < -10000)
            {
                player.horizontalPos = -10000;
            }
            if (player.horizontalPos > 10000)
            {
                player.horizontalPos = 10000;
            }

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
            player.horizontalVelocity = state.PlayerVelocityHorizontal[i];
            player.verticalVelocity = state.PlayerVelocityVertical[i];
        }
    }

    protected override SimulationState CreateState()
    {
        var simulationState = new SimulationState { 
            PlayerPositionsHorizontal = new int[AllClients.Count] , 
            PlayerPositionsVertical = new int[AllClients.Count],
            PlayerVelocityHorizontal = new int[AllClients.Count],
            PlayerVelocityVertical = new int[AllClients.Count]
        };
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            simulationState.PlayerPositionsHorizontal[i] = player.horizontalPos;
            simulationState.PlayerPositionsVertical[i] = player.verticalPos;
            simulationState.PlayerVelocityHorizontal[i] = player.horizontalVelocity;
            simulationState.PlayerVelocityVertical[i] = player.verticalVelocity;
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
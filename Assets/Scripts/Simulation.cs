using Coherence.Toolkit;
using UnityEngine;

public class Simulation : CoherenceInputSimulation<SimulationState>
{
    protected override void SetInputs(CoherenceClientConnection client)
    {
        var player = client.GameObject.GetComponent<Player>();
        player.SetMovement();
    }

    protected override void Simulate(long simulationFrame)
    {
        foreach (CoherenceClientConnection client in AllClients)
        {
            var player = client.GameObject.GetComponent<Player>();
            var movement = (int)player.GetMovement(simulationFrame);
            if (movement > 9 || movement < 1)
            {
                continue;
            }
            int horizontalMovement = (movement - 1) % 3 - 1;
            int verticalMovement = (movement - 1) / 3 - 1;
            player.horizontalPos += (int)((player.Speed * FixedTimeStep * horizontalMovement) * 1000);
            player.verticalPos += (int)((player.Speed * FixedTimeStep * verticalMovement) * 1000);
            //player.transform.position += player.Speed * FixedTimeStep * new Vector3(horizontalMovement, verticalMovement, 0);
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
        Player player = AllClients[0].GameObject.GetComponent<Player>();
        player.horizontalPos = -5000;
        player.verticalPos = 0;
        SimulationEnabled = AllClients.Count >= 2;
        if (SimulationEnabled)
        {
            Player player2 = AllClients[1].GameObject.GetComponent<Player>();
            player2.horizontalPos = 5000;
            player2.verticalPos = 0;
            StateStore.Clear();
        }
    }

    protected override void OnClientLeft(CoherenceClientConnection client)
    {
        SimulationEnabled = AllClients.Count >= 2;
    }
}
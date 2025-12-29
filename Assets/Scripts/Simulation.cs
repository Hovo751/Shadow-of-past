using Coherence.Toolkit;
using UnityEngine;

public class Simulation : CoherenceInputSimulation<SimulationState>
{
    //player1.posx > player2.posx --> player1 is looking left player2 is looking right
    public bool drawHitbox = true;
    [System.Serializable]
    public struct AnimationData
    {
        public AnimBase data;
        public int nextAnim;
    }
    [System.Serializable]
    public struct Character
    {
        public string name;
        public AnimationData[] data;
    }

    public Character[] characters;

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

    private void OnDrawGizmos()
    {
        if (drawHitbox)
        {
            int k = 0;
            foreach (CoherenceClientConnection client in AllClients)
            {
                Player player = client.GameObject.GetComponent<Player>();
                Player otherPlayer = null;
                if (k == 1)
                {
                    otherPlayer = AllClients[0].GameObject.GetComponent<Player>();
                }
                else if (k == 0)
                {
                    otherPlayer = AllClients[1].GameObject.GetComponent<Player>();
                }
                k++;
                AnimBase anim = characters[player.character].data[player.animationType].data;
                float dir = 1;
                if (player.horizontalPos > otherPlayer.horizontalPos)
                {
                    dir = -1;
                }
                Frame frame = anim.frames[player.animationFrame];
                //hurtbox
                Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                for (int i = 0; i < frame.hurtbox.Length; i++)
                {
                    Rectengale rect = frame.hurtbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //hitbox
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                for (int i = 0; i < frame.hitbox.Length; i++)
                {
                    Rectengale rect = frame.hitbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //collisionbox
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                for (int i = 0; i < frame.collisionBox.Length; i++)
                {
                    Rectengale rect = frame.collisionBox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //throwbox
                Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
                for (int i = 0; i < frame.throwbox.Length; i++)
                {
                    Rectengale rect = frame.throwbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
            }
        }
    }
}
using Coherence.Cloud;
using Coherence.Toolkit;
using Unity.VisualScripting;
using UnityEngine;

public class Simulation : CoherenceInputSimulation<SimulationState>
{
    //player1.posx > player2.posx --> player1 is looking left player2 is looking right
    public bool drawHitbox = true;
    [System.Serializable]
    public struct AnimationData
    {
        public string name;
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

    private bool isInAir(PlayerChangebleStats player)
    {
        if (player.PlayerPositionVertical <= 0)
        {
            return false;
        }
        return true;
    }
    private PlayerChangebleStats Jump(Player player, int horizontalDir)
    {
        PlayerChangebleStats result = new PlayerChangebleStats();
        result.PlayerVelocityVertical = player.JumpPower;
        result.PlayerVelocityHorizontal = player.JumpPowerSide * horizontalDir;
        return result;
    }
    private PlayerChangebleStats CalculatePerPlayer(int playerNumber, long simulationFrame)
    {
        Player player = AllClients[playerNumber].GameObject.GetComponent<Player>();
        PlayerChangebleStats result = player.changebleStats;
        Player otherPlayer = null;
        if (playerNumber == 1)
        {
            otherPlayer = AllClients[0].GameObject.GetComponent<Player>();
        }
        else if (playerNumber == 0)
        {
            otherPlayer = AllClients[1].GameObject.GetComponent<Player>();
        }

        int movement = (int)player.GetMovement(simulationFrame);
        if (movement > 9 || movement < 1)
        {
            return player.changebleStats;
        }
        int horizontalMovement = (movement - 1) % 3 - 1;
        int verticalMovement = (movement - 1) / 3 - 1;
        if (isInAir(result))
        {
            result.PlayerVelocityVertical -= player.Gravity;
        }
        else
        {
            result.PlayerVelocityVertical = 0;
            result.PlayerPositionVertical = 0;
            result.PlayerVelocityHorizontal = player.Speed * horizontalMovement;
            if (verticalMovement == 1)
            {
                PlayerChangebleStats p = Jump(player, horizontalMovement);
                result.PlayerVelocityVertical = p.PlayerVelocityVertical;
                result.PlayerVelocityHorizontal = p.PlayerVelocityHorizontal;
            }
        }

        result.PlayerPositionHorizontal += result.PlayerVelocityHorizontal;
        result.PlayerPositionVertical += result.PlayerVelocityVertical;

        if (result.PlayerPositionHorizontal < -10000)
        {
            result.PlayerPositionHorizontal = -10000;
        }
        if (result.PlayerPositionHorizontal  > 10000)
        {
            result.PlayerPositionHorizontal = 10000;
        }

        result.PlayerAnimationFrame++;

        if (characters[player.character].data[result.PlayerAnimation].data.frames.Length <= result.PlayerAnimationFrame)
        {
            result.PlayerAnimationFrame = 0;
            result.PlayerAnimation = result.PlayerNextAnimation;
            result.PlayerNextAnimation = characters[player.character].data[result.PlayerAnimation].nextAnim;
        }

        return result;
    }

    protected override void Simulate(long simulationFrame)
    {
        Player player1 = AllClients[0].GameObject.GetComponent<Player>();
        Player player2 = AllClients[1].GameObject.GetComponent<Player>();

        int movement = (int)player1.GetMovement(simulationFrame);
        int movement2 = (int)player2.GetMovement(simulationFrame);

        if (movement > 9 || movement < 1 || movement2 > 9 || movement2 < 1)
        {
            return;
        }
        //calculate

        PlayerChangebleStats player1Data = CalculatePerPlayer(0, simulationFrame);
        PlayerChangebleStats player2Data = CalculatePerPlayer(1, simulationFrame);

        player1.changebleStats = player1Data;
        player2.changebleStats = player2Data;
    }

    protected override void Rollback(long toFrame, SimulationState state)
    {
        Debug.Log("Rollback");
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            player.changebleStats = state.PlayerData[i];
        }
    }

    protected override SimulationState CreateState()
    {
        var simulationState = new SimulationState { 
            PlayerData = new PlayerChangebleStats[AllClients.Count] ,
        };
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            simulationState.PlayerData[i] = player.changebleStats;
        }

        return simulationState;
    }

    protected override void OnClientJoined(CoherenceClientConnection client)
    {
        Player player1 = AllClients[0].GameObject.GetComponent<Player>();
        player1.changebleStats.PlayerPositionHorizontal = -5000;
        player1.changebleStats.PlayerPositionVertical = 0;
        if (AllClients.Count >= 2)
        {
            Player player2 = AllClients[1].GameObject.GetComponent<Player>();
            player2.changebleStats.PlayerPositionHorizontal = 5000;
            player2.changebleStats.PlayerPositionVertical = 0;
            player1.changebleStats.PlayerAnimationFrame = 0;
            player2.changebleStats.PlayerAnimationFrame = 0;
            _camera.player1 = player1.transform;
            _camera.player2 = player2.transform;
            StateStore.Clear();
            SimulationEnabled = AllClients.Count >= 2;
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
                AnimBase anim = characters[player.character].data[player.changebleStats.PlayerAnimation].data;
                float dir = 1;
                if (player.changebleStats.PlayerPositionHorizontal > otherPlayer.changebleStats.PlayerPositionHorizontal)
                {
                    dir = -1;
                }
                Frame frame = anim.frames[player.changebleStats.PlayerAnimationFrame];
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
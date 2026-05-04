using Coherence.Cloud;
using Coherence.Toolkit;
using UnityEngine;
public static class Animations
{
    public const int Idle = 0;
    public const int WalkBack = 1;
    public const int WalkForward = 2;
    public const int JumpStraight = 3;
    public const int JumpBack = 5;
    public const int JumpForward = 4;
    public const int Crouch = 6;
    public const int CrouchBlock = 7;
    public const int DashForward = 8;
}

public class Simulation : CoherenceInputSimulation<SimulationState>
{

    public bool drawHitbox = true;

    public Characters characters;

    public Camera _camera;
    public bool CheckCollision(Rectengale a, Rectengale b)
    {
        return Mathf.Abs(a.posX - b.posX) * 2 < (a.sizeX + b.sizeX) &&
               Mathf.Abs(a.posY - b.posY) * 2 < (a.sizeY + b.sizeY);
    }
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
    private void PlayAnimation(ref PlayerChangebleStats result, Player player, int anim)
    {
        result.PlayerAnimationFrame = 0;
        result.PlayerAnimation = anim;
        result.PlayerNextAnimation = characters.characters[player.character].data[anim].nextAnim;
    }
    bool CanCancelInto(PlayerChangebleStats result, Player player, int anim)
    {
        if (result.PlayerAnimation != anim && characters.characters[player.character].data[result.PlayerAnimation].data.frames[result.PlayerAnimationFrame].cancelLvl <= characters.characters[player.character].data[1].data.cancelLvl && characters.characters[player.character].data[1].data.inAir == isInAir(result))
            return true;
        return false;
    }
    private void Jump(ref PlayerChangebleStats result, Player player, int horizontalDir)
    {
        result.PlayerVelocityVertical = player.JumpPower;
        result.PlayerVelocityHorizontal = player.JumpPowerSide * horizontalDir;
    }
    private PlayerChangebleStats CalculatePerPlayer(int playerNumber, long simulationFrame)
    {
        Player player = AllClients[playerNumber].GameObject.GetComponent<Player>();
        PlayerChangebleStats result = player.changebleStats;
        Player otherPlayer = null;

        int movement = (int)player.GetMovement(simulationFrame);
        if (movement > 9 || movement < 1)
        {
            return player.changebleStats;
        }
        int horizontalMovement = (movement - 1) % 3 - 1;
        int verticalMovement = (movement - 1) / 3 - 1;
        result.IsInAir = isInAir(result);
        //if (result.IsInAir)
        //{
        //    result.PlayerVelocityVertical -= player.Gravity;
        //}
        //else
        //{
        //    result.PlayerVelocityVertical = 0;
        //    result.PlayerPositionVertical = 0;
        //    result.PlayerVelocityHorizontal = player.Speed * horizontalMovement;
        //    if (verticalMovement == 1)
        //    {
        //        Jump(ref result, player, horizontalMovement);
        //        PlayAnimation(ref result, player, 3);
        //    }
        //}

        if (result.PlayerPositionHorizontal < -10000)
        {
            result.PlayerPositionHorizontal = -10000;
        }
        if (result.PlayerPositionHorizontal  > 10000)
        {
            result.PlayerPositionHorizontal = 10000;
        }

        int direction = 1;

        if (!result.IsLookingRight)
        {
            if (movement % 3 == 1)
            {
                movement += 2;
            }
            else if (movement % 3 == 0)
            {
                movement -= 2;
            }
            direction = -1;
        }
        
        AnimBase currentAnimation = characters.characters[player.character].data[result.PlayerAnimation].data;

        result.PlayerAnimationFrame++;

        if (currentAnimation.frames.Length <= result.PlayerAnimationFrame)
        {
            PlayAnimation(ref result, player, result.PlayerNextAnimation);
        }
        if (CanCancelInto(result, player, Animations.WalkBack) && movement == 4)
        {
            PlayAnimation(ref result, player, Animations.WalkBack);
        }
        else if (CanCancelInto(result, player, Animations.WalkForward) && movement == 6)
        {
            PlayAnimation(ref result, player, Animations.WalkForward);
        }
        else if (CanCancelInto(result, player, Animations.Crouch) && (movement == 2 || movement == 3))
        {
            PlayAnimation(ref result, player, Animations.Crouch);
        }
        else if (CanCancelInto(result, player, Animations.CrouchBlock) && movement == 1)
        {
            PlayAnimation(ref result, player, Animations.CrouchBlock);
        }
        else if (CanCancelInto(result, player, Animations.JumpStraight) && movement == 8)
        {
            PlayAnimation(ref result, player, Animations.JumpStraight);
        }
        else if (CanCancelInto(result, player, Animations.JumpForward) && movement == 9)
        {
            PlayAnimation(ref result, player, Animations.JumpForward);
        }
        else if (CanCancelInto(result, player, Animations.JumpBack) && movement == 7)
        {
            PlayAnimation(ref result, player, Animations.JumpBack);
        }
        else if (CanCancelInto(result, player, Animations.Idle) && movement == 5)
        {
            PlayAnimation(ref result, player, Animations.Idle);
        }

        currentAnimation = characters.characters[player.character].data[result.PlayerAnimation].data;

        if (currentAnimation.frames[result.PlayerAnimationFrame].setVelocityHbool)
            result.PlayerVelocityHorizontal = currentAnimation.frames[result.PlayerAnimationFrame].setVelocityHorizontal * direction;
        if (currentAnimation.frames[result.PlayerAnimationFrame].setVelocityVbool)
            result.PlayerVelocityVertical = currentAnimation.frames[result.PlayerAnimationFrame].setVelocityVertical;
        if (currentAnimation.frames[result.PlayerAnimationFrame].setAccelerationHbool)
            result.PlayerAccelerationHorizontal = currentAnimation.frames[result.PlayerAnimationFrame].setAccelerationHorizontal * direction;
        if (currentAnimation.frames[result.PlayerAnimationFrame].setAccelerationVbool)
            result.PlayerAccelerationVertical = currentAnimation.frames[result.PlayerAnimationFrame].setAccelerationVertical;

        result.PlayerVelocityHorizontal += result.PlayerAccelerationHorizontal;
        result.PlayerVelocityVertical += result.PlayerAccelerationVertical;
        result.PlayerPositionHorizontal += result.PlayerVelocityHorizontal;
        result.PlayerPositionVertical += result.PlayerVelocityVertical;
        result.PlayerPositionHorizontal += currentAnimation.frames[result.PlayerAnimationFrame].addPosX * direction;
        result.PlayerPositionVertical += currentAnimation.frames[result.PlayerAnimationFrame].addPosY;

        if (result.PlayerPositionVertical < 0)
        {
            result.PlayerPositionVertical = 0;
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

        if (!isInAir(player1Data) && characters.characters[player1.character].data[player1Data.PlayerAnimation].data.frames[player1Data.PlayerAnimationFrame].cancelLvl == 0)
        {
            if (player1Data.PlayerPositionHorizontal > player2Data.PlayerPositionHorizontal)
            {
                player1Data.IsLookingRight = false;
            }
            else
            {
                player1Data.IsLookingRight = true;
            }
        }

        if (!isInAir(player2Data) && characters.characters[player2.character].data[player2Data.PlayerAnimation].data.frames[player2Data.PlayerAnimationFrame].cancelLvl == 0)
        {
            if (player1Data.PlayerPositionHorizontal > player2Data.PlayerPositionHorizontal)
            {
                player2Data.IsLookingRight = true;
            }
            else
            {
                player2Data.IsLookingRight = false;
            }
        }

        int player1Dir = 1;

        if (!player1Data.IsLookingRight)
        {
            player1Dir = -1;
        }

        int player2Dir = 1;

        if (!player2Data.IsLookingRight)
        {
            player2Dir = -1;
        }

        Rectengale player1Collision = characters.characters[player1.character].data[player1Data.PlayerAnimation].data.frames[player1Data.PlayerAnimationFrame].collisionBox;
        Rectengale player2Collision = characters.characters[player2.character].data[player2Data.PlayerAnimation].data.frames[player2Data.PlayerAnimationFrame].collisionBox;
        player1Collision.posX *= player1Dir;
        player2Collision.posX *= player2Dir;
        player1Collision.posX += player1Data.PlayerPositionHorizontal;
        player1Collision.posY += player1Data.PlayerPositionVertical;
        player2Collision.posX += player2Data.PlayerPositionHorizontal;
        player2Collision.posY += player2Data.PlayerPositionVertical;
        if (CheckCollision(player1Collision, player2Collision))
        {
            Rectengale player1CollisionCopy = characters.characters[player1.character].data[player1Data.PlayerAnimation].data.frames[player1Data.PlayerAnimationFrame].collisionBox;
            Rectengale player2CollisionCopy = characters.characters[player2.character].data[player2Data.PlayerAnimation].data.frames[player2Data.PlayerAnimationFrame].collisionBox;
            int midle = (player1Collision.posX + player2Collision.posX) / 2;
            if (player1Collision.posX < midle)
            {
                player1Data.PlayerPositionHorizontal = midle - player1CollisionCopy.sizeX / 2 - player1CollisionCopy.posX * player1Dir;
            }
            else if (player1Collision.posX > midle)
            {
                player1Data.PlayerPositionHorizontal = midle + player1CollisionCopy.sizeX / 2 - player1CollisionCopy.posX * player1Dir;
            }
            if (player2Collision.posX < midle)
            {
                player2Data.PlayerPositionHorizontal = midle - player2CollisionCopy.sizeX / 2 - player2CollisionCopy.posX * player2Dir;
            }
            else if (player2Collision.posX > midle)
            {
                player2Data.PlayerPositionHorizontal = midle + player2CollisionCopy.sizeX / 2 - player2CollisionCopy.posX * player2Dir;
            }
        }

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
                AnimBase anim = characters.characters[player.character].data[player.changebleStats.PlayerAnimation].data;
                float dir = 1;
                if (!player.changebleStats.IsLookingRight)
                {
                    dir = -1;
                }
                Rectengale rect;
                Frame frame = anim.frames[player.changebleStats.PlayerAnimationFrame];
                //hurtbox
                Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                for (int i = 0; i < frame.hurtbox.Length; i++)
                {
                    rect = frame.hurtbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //hitbox
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                for (int i = 0; i < frame.hitbox.Length; i++)
                {
                    rect = frame.hitbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //collisionbox
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                rect = frame.collisionBox;
                Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));

                //throwbox
                Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
                for (int i = 0; i < frame.throwbox.Length; i++)
                {
                    rect = frame.throwbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //blockbox
                Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
                for (int i = 0; i < frame.blockbox.Length; i++)
                {
                    rect = frame.blockbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
            }
        }
    }
}
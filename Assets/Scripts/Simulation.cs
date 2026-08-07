using Coherence.Cloud;
using Coherence.Toolkit;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public static class Animations
{
    public const int Idle = 0;
    public const int WalkBack = 1;
    public const int WalkForward = 2;
    public const int JumpStraight = 3;
    public const int JumpForward = 4;
    public const int JumpBack = 5;
    public const int Crouch = 6;
    public const int CrouchBlock = 7;
    public const int DashForward = 8;
    public const int DashBackward = 9;
    public const int GetHitUp = 10;
    public const int GetHitDown = 11;
    public const int BlockHigh = 12;
    public const int BlockLow = 13;
    public const int Light = 14;
    public const int Medium = 15;
    public const int Heavy = 16;
    public const int LightCrouch = 17;

}

public class Simulation : CoherenceInputSimulation<SimulationState>
{
    // IF SMTH EXPLODES THAN GO TO LINE 1361 IN COHERENCEBRIDGE
    public ComboCounter ComboCounter;
    Dictionary<long, SimulationState> history = new Dictionary<long, SimulationState>();
    public struct MovementInput
    {
        public int input;
        public int holdTime;
    }
    public struct ButtonInput
    {
        public bool buttonDown;
        public int holdTime;
    }
    //public struct ButtonsInputs
    //{
    //    public bool light;
    //    public bool medium;
    //    public bool heavy;
    //}
    public struct MovementAndButtonInput
    {
        public int movement;
        public bool light;
        public bool medium;
        public bool heavy;
    }
    private long startFrame = -1;
    private int skipFrames = 0;
    public bool drawHitbox = true;
    public int frameBufferSize;
    public TextMeshProUGUI txtDebug;

    public Characters characters;

    public Camera _camera;
    public bool CheckCollision(Rectengale a, Rectengale b)
    {
        return Mathf.Abs(a.posX - b.posX) * 2 < (a.sizeX + b.sizeX) &&
               Mathf.Abs(a.posY - b.posY) * 2 < (a.sizeY + b.sizeY);
    }
    public bool CheckCollision(Rectengale a, Rectengale b, out int centerX, out int centerY)
    {
        centerX = 0;
        centerY = 0;

        if (!CheckCollision(a, b))
            return false;

        int aLeft = a.posX - a.sizeX / 2;
        int aRight = a.posX + a.sizeX / 2;
        int aBottom = a.posY - a.sizeY / 2;
        int aTop = a.posY + a.sizeY / 2;

        int bLeft = b.posX - b.sizeX / 2;
        int bRight = b.posX + b.sizeX / 2;
        int bBottom = b.posY - b.sizeY / 2;
        int bTop = b.posY + b.sizeY / 2;

        int overlapLeft = Math.Max(aLeft, bLeft);
        int overlapRight = Math.Min(aRight, bRight);

        int overlapBottom = Math.Max(aBottom, bBottom);
        int overlapTop = Math.Min(aTop, bTop);

        centerX = (overlapLeft + overlapRight) / 2;
        centerY = (overlapBottom + overlapTop) / 2;

        return true;
    }
    protected override void SetInputs(CoherenceClientConnection client)
    {
        var player = client.GameObject.GetComponent<Player>();
        player.SetInput();
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
        result.PlayerNextAnimation = characters.characters[player.character].animations[anim].nextAnim;
        result.HitLanded = 0;
    }
    bool CanCancelInto(PlayerChangebleStats result, Player player, int anim)
    {
        AnimBase currentAnim = characters.characters[player.character].animations[result.PlayerAnimation].data;
        AnimBase nextAnim = characters.characters[player.character].animations[anim].data;
        if (result.PlayerAnimation != anim &&
            currentAnim.frames[result.PlayerAnimationFrame].cancelLvl[Mathf.Min(result.HitLanded, currentAnim.frames[result.PlayerAnimationFrame].cancelLvl.Length - 1)] <= nextAnim.cancelLvl &&
            currentAnim.inAir == isInAir(result))
            return true;
        return false;
    }
    MovementInput[] SortInputs(int[] arr)
    {
        int prevInput = -1;
        List<MovementInput> movementBuffer = new List<MovementInput>();

        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == prevInput)
            {
                MovementInput input = movementBuffer[movementBuffer.Count - 1];
                input.holdTime++;
                movementBuffer[movementBuffer.Count - 1] = input;
            }
            else
            {
                MovementInput input = new MovementInput();
                input.input = arr[i];
                input.holdTime = 1;

                movementBuffer.Add(input);
                prevInput = arr[i];
            }
        }

        //for (int i = 0; i < movementBuffer.Count; i++)
        //{
        //    if (movementBuffer[i].holdTime == 64) break;
        //    Debug.Log(
        //        "Input: " + movementBuffer[i].input +
        //        " HoldTime: " + movementBuffer[i].holdTime
        //    );
        //}

        return movementBuffer.ToArray();
    }
    ButtonInput[] SortInputs(bool[] arr, long startFrame)
    {
        bool prevInput = false;
        List<ButtonInput> movementBuffer = new List<ButtonInput>();

        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == prevInput && i != 0)
            {
                ButtonInput input = movementBuffer[movementBuffer.Count - 1];
                input.holdTime++;
                movementBuffer[movementBuffer.Count - 1] = input;
            }
            else
            {
                ButtonInput input = new ButtonInput();
                input.buttonDown = arr[i];
                input.holdTime = 1;

                movementBuffer.Add(input);
                prevInput = arr[i];
            }
            if (startFrame - i > startFrame && startFrame != -1 && i != 0)
            {
                if (history[startFrame - i].skipFrames > 0)
                {
                    ButtonInput input = movementBuffer[movementBuffer.Count - 1];
                    input.holdTime--;
                    movementBuffer[movementBuffer.Count - 1] = input;
                }
            }
        }

        return movementBuffer.ToArray();
    }
    private PlayerChangebleStats CalculatePerPlayer(int playerNumber, long simulationFrame)
    {
        Player player = AllClients[playerNumber].GameObject.GetComponent<Player>();
        PlayerChangebleStats result = player.changebleStats;

        //Obtaining the players input

        MovementAndButtonInput input = player.GetInput(simulationFrame);
        int movement = (int)input.movement;

        int[] movementBufferNotSorted = new int[64];
        bool[] lightButtonBufferNotSorted = new bool[64];
        bool[] mediumButtonBufferNotSorted = new bool[64];
        bool[] heavyButtonBufferNotSorted = new bool[64];
        if (movement > 9 || movement < 1)
        {
            return player.changebleStats;
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
        for (long i = simulationFrame; i > simulationFrame - 64; i--)
        {
            if (i >= startFrame)
            {
                MovementAndButtonInput inputPast = player.GetInput(i);
                int m = (int)inputPast.movement;
                if (!result.IsLookingRight)
                {
                    if (m % 3 == 1)
                    {
                        m += 2;
                    }
                    else if (m % 3 == 0)
                    {
                        m -= 2;
                    }
                    direction = -1;
                }
                movementBufferNotSorted[simulationFrame - i] = m;
                lightButtonBufferNotSorted[simulationFrame - i] = inputPast.light;
                mediumButtonBufferNotSorted[simulationFrame - i] = inputPast.medium;
                heavyButtonBufferNotSorted[simulationFrame - i] = inputPast.heavy;
            }
            else
            {
                movementBufferNotSorted[simulationFrame - i] = 5;
                lightButtonBufferNotSorted[simulationFrame - i] = false;
                mediumButtonBufferNotSorted[simulationFrame - i] = false;
                heavyButtonBufferNotSorted[simulationFrame - i] = false;
            }
        }

        MovementInput[] movementBuffer = SortInputs(movementBufferNotSorted);
        ButtonInput[] lightButtonBuffer = SortInputs(lightButtonBufferNotSorted, simulationFrame);
        ButtonInput[] mediumButtonBuffer = SortInputs(mediumButtonBufferNotSorted, simulationFrame);
        ButtonInput[] heavyButtonBuffer = SortInputs(heavyButtonBufferNotSorted, simulationFrame);

        result.IsInAir = isInAir(result);

        //Stepping over to next frame of animation and changing it based on the inputs

        AnimBase currentAnimation = characters.characters[player.character].animations[result.PlayerAnimation].data;

        result.PlayerAnimationFrame++;

        if (result.PlayerAnimation == Animations.GetHitUp || result.PlayerAnimation == Animations.GetHitDown || 
            result.PlayerAnimation == Animations.BlockHigh || result.PlayerAnimation == Animations.BlockLow)
        {
            if (result.PlayerAnimationFrame >= currentAnimation.frames.Length)
            {
                result.PlayerAnimationFrame = currentAnimation.frames.Length - 1;
            }
            result.InHitstun--;
            if (result.InHitstun <= 0)
            {
                result.PlayerAnimation = Animations.Idle;
                result.PlayerAnimationFrame = 0;
                result.InHitstun = 0;
            }
        }

        if (currentAnimation.frames.Length <= result.PlayerAnimationFrame && result.PlayerAnimation != Animations.GetHitUp)
        {
            PlayAnimation(ref result, player, result.PlayerNextAnimation);
        }
        if (heavyButtonBuffer[0].buttonDown == true && CanCancelInto(result, player, Animations.Heavy) && heavyButtonBuffer[0].holdTime < frameBufferSize)
        {
            PlayAnimation(ref result, player, Animations.Heavy);
        }
        else if (mediumButtonBuffer[0].buttonDown == true && CanCancelInto(result, player, Animations.Medium) && mediumButtonBuffer[0].holdTime < frameBufferSize)
        {
            PlayAnimation(ref result, player, Animations.Medium);
        }
        else if (lightButtonBuffer[0].buttonDown == true && lightButtonBuffer[0].holdTime < frameBufferSize)
        {
            if (CanCancelInto(result, player, Animations.Light) && movement != 1 && movement != 2 && movement != 3)
                PlayAnimation(ref result, player, Animations.Light);
            else if (CanCancelInto(result, player, Animations.LightCrouch) && (movement == 1 || movement == 2 || movement == 3))
                PlayAnimation(ref result, player, Animations.LightCrouch);
        }
        if (movement == 4)
        {
            if (movementBuffer.Length > 3 &&
                    movementBuffer[0].holdTime <= 10 &&
                    movementBuffer[1].input == 5 && movementBuffer[1].holdTime <= 10 &&
                    movementBuffer[2].input == 4 && movementBuffer[2].holdTime <= 10 &&
                    CanCancelInto(result, player, Animations.DashBackward))
            {
                PlayAnimation(ref result, player, Animations.DashBackward);
            }
            else if (CanCancelInto(result, player, Animations.WalkBack))
            {

                PlayAnimation(ref result, player, Animations.WalkBack);
            }
        }
        else if (movement == 6)
        {
            if (movementBuffer.Length > 3 &&
                    movementBuffer[0].holdTime <= 10 &&
                    movementBuffer[1].input == 5 && movementBuffer[1].holdTime <= 10 &&
                    movementBuffer[2].input == 6 && movementBuffer[2].holdTime <= 10 &&
                    CanCancelInto(result, player, Animations.DashForward))
            {
                PlayAnimation(ref result, player, Animations.DashForward);
            }
            else if (CanCancelInto(result, player, Animations.WalkForward))
            {

                PlayAnimation(ref result, player, Animations.WalkForward);
            }
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

        currentAnimation = characters.characters[player.character].animations[result.PlayerAnimation].data;

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

        if (result.PlayerAnimation == Animations.GetHitUp || result.PlayerAnimation == Animations.GetHitDown ||
            result.PlayerAnimation == Animations.BlockHigh || result.PlayerAnimation == Animations.BlockLow)
        {
            if (result.PlayerVelocityHorizontal / direction > 0)
            {
                result.PlayerVelocityHorizontal = 0;
            }
        }

        result.PlayerPositionHorizontal += result.PlayerVelocityHorizontal;
        result.PlayerPositionVertical += result.PlayerVelocityVertical;
        result.PlayerPositionHorizontal += currentAnimation.frames[result.PlayerAnimationFrame].addPosX * direction;
        result.PlayerPositionVertical += currentAnimation.frames[result.PlayerAnimationFrame].addPosY;

        //Making sure that the player doesn't fall of the ground

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

        int player1MovementInput = (int)player1.GetInput(simulationFrame).movement;
        int player2MovementInput = (int)player2.GetInput(simulationFrame).movement;

        if (player1MovementInput > 9 || player1MovementInput < 1 || player2MovementInput > 9 || player2MovementInput < 1 || startFrame == -1 || startFrame >= simulationFrame)
        {
            return;
        }
        SimulationState currentState = new SimulationState
        {
            PlayerData = new PlayerChangebleStats[AllClients.Count],
            skipFrames = skipFrames,
        };

        if (skipFrames > 0) { 
            skipFrames--;
            try
            {
                history.Add(simulationFrame, currentState);
            }
            catch (ArgumentException)
            {
                history[simulationFrame] = currentState;
            }
            return;
        }

        //Calculating the players position based on velocity and acceleration and also playing animations based on players inputs

        PlayerChangebleStats player1Data = CalculatePerPlayer(0, simulationFrame);
        PlayerChangebleStats player2Data = CalculatePerPlayer(1, simulationFrame);

        //Setting which way the players look

        Frame player1Frame = characters.characters[player1.character].animations[player1Data.PlayerAnimation].data.frames[player1Data.PlayerAnimationFrame];
        Frame player2Frame = characters.characters[player2.character].animations[player2Data.PlayerAnimation].data.frames[player2Data.PlayerAnimationFrame];

        if (!isInAir(player1Data) && (player1Frame.cancelLvl[Mathf.Min(player1Data.HitLanded, player1Frame.cancelLvl.Length - 1)] == 0 || player1Frame.canRotate))
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

        if (!isInAir(player2Data) && (player2Frame.cancelLvl[Mathf.Min(player2Data.HitLanded, player2Frame.cancelLvl.Length - 1)] == 0 || player2Frame.canRotate))
        {
            if (player2Data.PlayerPositionHorizontal > player1Data.PlayerPositionHorizontal)
            {
                player2Data.IsLookingRight = false;
            }
            else
            {
                player2Data.IsLookingRight = true;
            }
        }

        int player1Dir = 1;

        if (!player1Data.IsLookingRight)
        {
            if (player1MovementInput % 3 == 1)
            {
                player1MovementInput += 2;
            }
            else if (player1MovementInput % 3 == 0)
            {
                player1MovementInput -= 2;
            }
            player1Dir = -1;
        }

        int player2Dir = 1;

        if (!player2Data.IsLookingRight)
        {
            if (player2MovementInput % 3 == 1)
            {
                player2MovementInput += 2;
            }
            else if (player2MovementInput % 3 == 0)
            {
                player2MovementInput -= 2;
            }
            player2Dir = -1;
        }

        //Collision Box collision check (aka. make sure that the players are far enough from each other)

        Rectengale player1Collision = player1Frame.collisionBox;
        Rectengale player2Collision = player2Frame.collisionBox;
        player1Collision.posX *= player1Dir;
        player2Collision.posX *= player2Dir;
        player1Collision.posX += player1Data.PlayerPositionHorizontal;
        player1Collision.posY += player1Data.PlayerPositionVertical;
        player2Collision.posX += player2Data.PlayerPositionHorizontal;
        player2Collision.posY += player2Data.PlayerPositionVertical;
        if (CheckCollision(player1Collision, player2Collision))
        {
            Rectengale player1CollisionCopy = player1Frame.collisionBox;
            Rectengale player2CollisionCopy = player2Frame.collisionBox;
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

        //Detecting if anyone landed a hit

        int player1Gothit = -1;
        int player2Gothit = -1;
        int player1HitX = 0;
        int player2HitX = 0;
        int player1HitY = 0;
        int player2HitY = 0; 

        if (player1Data.HitLanded < player1Frame.hitboxLand)
        {
            for (int i = 0; i < player1Frame.hitbox.Length; i++)
            {
                for (int j = 0; j < player2Frame.hurtbox.Length; j++)
                {
                    player1Collision = player1Frame.hitbox[i];
                    player2Collision = player2Frame.hurtbox[j];
                    player1Collision.posX *= player1Dir;
                    player2Collision.posX *= player2Dir;
                    player1Collision.posX += player1Data.PlayerPositionHorizontal;
                    player1Collision.posY += player1Data.PlayerPositionVertical;
                    player2Collision.posX += player2Data.PlayerPositionHorizontal;
                    player2Collision.posY += player2Data.PlayerPositionVertical;

                    if (CheckCollision(player1Collision, player2Collision, out player2HitX, out player2HitY))
                    {
                        int isBlocking = -1;

                        if (player2MovementInput == 4 && 
                            (player2Data.PlayerAnimation == Animations.WalkBack || 
                            player2Data.PlayerAnimation == Animations.BlockHigh || 
                            player2Data.PlayerAnimation == Animations.BlockLow))
                        {
                            Debug.Log(player2Data.PlayerAnimation);
                            player2Collision = characters.characters[player2.character].blockHightBox;
                            isBlocking = Animations.BlockHigh;
                        }
                        else if (player2MovementInput == 1 && 
                            (player2Data.PlayerAnimation == Animations.CrouchBlock ||
                            player2Data.PlayerAnimation == Animations.BlockHigh || 
                            player2Data.PlayerAnimation == Animations.BlockLow))
                        {
                            player2Collision = characters.characters[player2.character].blockLowBox;
                            isBlocking = Animations.BlockLow;
                        }

                        player2Collision = characters.characters[player2.character].blockHightBox;
                        player2Collision.posX *= player2Dir;
                        player2Collision.posX += player2Data.PlayerPositionHorizontal;
                        player2Collision.posY += player2Data.PlayerPositionVertical;

                        int didBlock = -1;

                        if (isBlocking != -1 && CheckCollision(player1Collision, player2Collision))
                        {
                            didBlock = isBlocking;
                        }

                        if (player1Collision.posY > player2Frame.collisionBox.posY && didBlock == -1)
                        {
                            player2Gothit = Animations.GetHitUp;
                        }
                        else if (didBlock == -1)
                        {
                            player2Gothit = Animations.GetHitDown;
                        }
                        else
                        {
                            player2Gothit = didBlock;
                        }
                        break;
                    }
                }
                if (player2Gothit != -1)
                    break;
            }
        }

        if (player2Data.HitLanded < player2Frame.hitboxLand)
        {
            for (int i = 0; i < player2Frame.hitbox.Length; i++)
            {
                for (int j = 0; j < player1Frame.hurtbox.Length; j++)
                {
                    player2Collision = player2Frame.hitbox[i];
                    player1Collision = player1Frame.hurtbox[j];
                    player2Collision.posX *= player2Dir;
                    player1Collision.posX *= player1Dir;
                    player2Collision.posX += player2Data.PlayerPositionHorizontal;
                    player2Collision.posY += player2Data.PlayerPositionVertical;
                    player1Collision.posX += player1Data.PlayerPositionHorizontal;
                    player1Collision.posY += player1Data.PlayerPositionVertical;

                    if (CheckCollision(player2Collision, player1Collision, out player1HitX, out player1HitY))
                    {
                        Debug.Log("Checking If Blocking");
                        int isBlocking = -1;

                        if (player1MovementInput == 4 &&
                            (player1Data.PlayerAnimation == Animations.WalkBack ||
                             player1Data.PlayerAnimation == Animations.BlockHigh ||
                             player1Data.PlayerAnimation == Animations.BlockLow))
                        {
                            Debug.Log(player1Data.PlayerAnimation);
                            player1Collision = characters.characters[player1.character].blockHightBox;
                            isBlocking = Animations.BlockHigh;
                        }
                        else if (player1MovementInput == 1 &&
                            (player1Data.PlayerAnimation == Animations.CrouchBlock ||
                             player1Data.PlayerAnimation == Animations.BlockHigh ||
                             player1Data.PlayerAnimation == Animations.BlockLow))
                        {
                            Debug.Log("Checking If Blocking Low");
                            player1Collision = characters.characters[player1.character].blockLowBox;
                            isBlocking = Animations.BlockLow;
                        }

                        player1Collision = characters.characters[player1.character].blockHightBox;
                        player1Collision.posX *= player1Dir;
                        player1Collision.posX += player1Data.PlayerPositionHorizontal;
                        player1Collision.posY += player1Data.PlayerPositionVertical;

                        int didBlock = -1;

                        if (isBlocking != -1 && CheckCollision(player2Collision, player1Collision))
                        {
                            Debug.Log("Checking If Blocking Collided");
                            didBlock = isBlocking;
                        }

                        if (player2Collision.posY > player1Frame.collisionBox.posY && didBlock == -1)
                        {
                            player1Gothit = Animations.GetHitUp;
                        }
                        else if (didBlock == -1)
                        {
                            player1Gothit = Animations.GetHitDown;
                        }
                        else
                        {
                            player1Gothit = didBlock;
                        }
                        break;
                    }
                }

                if (player1Gothit != -1)
                    break;
            }
        }
        if (player1Data.InHitstun == 0 && player1Data.PlayerAnimation != Animations.BlockLow && player1Data.PlayerAnimation != Animations.BlockHigh)
        {
            player2Data.Combo = 0;
        }
        if (player2Data.InHitstun == 0 && player2Data.PlayerAnimation != Animations.BlockLow && player2Data.PlayerAnimation != Animations.BlockHigh)
        {
            player1Data.Combo = 0;
        }

        //Apply the required data if pl1 or pl2 got hit

        if (player1Gothit != -1)
        {
            if (player1Gothit == Animations.BlockHigh || player1Gothit == Animations.BlockLow)
            {
                Debug.Log("Player1 got blocked");
                player1Data.InHitstun = player2Frame.hitboxBlockStun;
                player1Data.PlayerAnimation = player1Gothit;
                player1Data.PlayerAnimationFrame = 0;
                player1Data.PlayerNextAnimation = Animations.Idle;
                if (player1Data.PlayerPositionHorizontal <= -9990 || player1Data.PlayerPositionHorizontal >= 9990)
                {
                    player2Data.PlayerVelocityHorizontal = -player2Frame.hitboxPushOnBlock * player2Dir;
                    player2Data.PlayerAccelerationHorizontal = player2Data.PlayerVelocityHorizontal / -12;
                }
                else
                {
                    player1Data.PlayerVelocityHorizontal = player2Frame.hitboxPushOnBlock * player2Dir;
                    player1Data.PlayerAccelerationHorizontal = player1Data.PlayerVelocityHorizontal / -12;
                }
                player2Data.HitLanded = player2Frame.hitboxLand;
                skipFrames = player2Frame.hitStopOnBlock;
            }
            else
            {
                Debug.Log("Player1 got hit");
                player2Data.Combo++;
                player1Data.InHitstun = player2Frame.hitboxHitStun;
                player1Data.PlayerAnimation = player1Gothit;
                player1Data.PlayerAnimationFrame = 0;
                player1Data.PlayerNextAnimation = Animations.Idle;
                if (player1Data.PlayerPositionHorizontal <= -9990 || player1Data.PlayerPositionHorizontal >= 9990)
                {
                    player2Data.PlayerVelocityHorizontal = -player2Frame.hitboxPush * player2Dir;
                    player2Data.PlayerAccelerationHorizontal = player2Data.PlayerVelocityHorizontal / -12;
                }
                else
                {
                    player1Data.PlayerVelocityHorizontal = player2Frame.hitboxPush * player2Dir;
                    player1Data.PlayerAccelerationHorizontal = player1Data.PlayerVelocityHorizontal / -12;
                }
                player2Data.HitLanded = player2Frame.hitboxLand;
                player2.PlayHit(new Vector3(player1HitX, player1HitY));
                skipFrames = player2Frame.hitStop;
            }
        }

        if (player2Gothit != -1)
        {
            if (player2Gothit == Animations.BlockHigh || player2Gothit == Animations.BlockLow)
            {
                Debug.Log("Player2 got blocked");
                player2Data.InHitstun = player1Frame.hitboxBlockStun;
                player2Data.PlayerAnimation = player2Gothit;
                player2Data.PlayerAnimationFrame = 0;
                player2Data.PlayerNextAnimation = Animations.Idle;
                if (player2Data.PlayerPositionHorizontal <= -9990 || player2Data.PlayerPositionHorizontal >= 9990)
                {
                    player1Data.PlayerVelocityHorizontal = -player1Frame.hitboxPushOnBlock * player1Dir;
                    player1Data.PlayerAccelerationHorizontal = player1Data.PlayerVelocityHorizontal / -12;
                }
                else
                {
                    player2Data.PlayerVelocityHorizontal = player1Frame.hitboxPushOnBlock * player1Dir;
                    player2Data.PlayerAccelerationHorizontal = player2Data.PlayerVelocityHorizontal / -12;
                }
                player1Data.HitLanded = player1Frame.hitboxLand;
                if (skipFrames < player1Frame.hitStopOnBlock)
                    skipFrames = player1Frame.hitStopOnBlock;
            }
            else
            {
                Debug.Log("Player2 got hit");
                player1Data.Combo++;
                player2Data.InHitstun = player1Frame.hitboxHitStun;
                player2Data.PlayerAnimation = player2Gothit;
                player2Data.PlayerAnimationFrame = 0;
                player2Data.PlayerNextAnimation = Animations.Idle;
                if (player2Data.PlayerPositionHorizontal <= -9990 || player2Data.PlayerPositionHorizontal >= 9990)
                {
                    player1Data.PlayerVelocityHorizontal = -player1Frame.hitboxPush * player1Dir;
                    player1Data.PlayerAccelerationHorizontal = player1Data.PlayerVelocityHorizontal / -12;
                }
                else
                {
                    player2Data.PlayerVelocityHorizontal = player1Frame.hitboxPush * player1Dir;
                    player2Data.PlayerAccelerationHorizontal = player2Data.PlayerVelocityHorizontal / -12;
                }
                player1Data.HitLanded = player1Frame.hitboxLand;
                player1.PlayHit(new Vector3(player2HitX, player2HitY));
                if (skipFrames < player1Frame.hitStop)
                    skipFrames = player1Frame.hitStop;
            }
        }

        //Making sure that the players are not out of bounds

        if (player1Data.PlayerPositionHorizontal < -10000)
        {
            player1Data.PlayerPositionHorizontal = -10000;
        }
        if (player1Data.PlayerPositionHorizontal > 10000)
        {
            player1Data.PlayerPositionHorizontal = 10000;
        }

        if (player2Data.PlayerPositionHorizontal < -10000)
        {
            player2Data.PlayerPositionHorizontal = -10000;
        }
        if (player2Data.PlayerPositionHorizontal > 10000)
        {
            player2Data.PlayerPositionHorizontal = 10000;
        }

        //Applying the changes

        player1.changebleStats = player1Data;
        player2.changebleStats = player2Data;

        //saving to history
        currentState = new SimulationState
        {
            PlayerData = new PlayerChangebleStats[AllClients.Count],
            skipFrames = skipFrames,
        };
        try
        {
            history.Add(simulationFrame, currentState);
        }
        catch (ArgumentException)
        {
            history[simulationFrame] = currentState;
        }
    }

    protected override void Rollback(long toFrame, SimulationState state)
    {
        try
        {
            history.Add(toFrame, state);
        }
        catch (ArgumentException)
        {
            history[toFrame] = state;
        }
        Debug.Log("Rollback");
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            player.changebleStats = state.PlayerData[i];
        }
        skipFrames = state.skipFrames;
    }

    protected override SimulationState CreateState()
    {
        var simulationState = new SimulationState { 
            PlayerData = new PlayerChangebleStats[AllClients.Count] ,
            skipFrames = skipFrames,
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
            ComboCounter.player1 = player1;
            ComboCounter.player2 = player2;
            _camera.player1 = player1.transform;
            _camera.player2 = player2.transform;
            StateStore.Clear();
            SimulationEnabled = AllClients.Count >= 2;
            if (player1.startFrame == -1 && player2.startFrame == -1)
            {
                startFrame = CurrentSimulationFrame + 120;
                player1.startFrame = startFrame;
                player2.startFrame = startFrame;
            }
            else
            {
                startFrame = player1.startFrame;
                if (startFrame == -1)
                {
                    startFrame = player2.startFrame;
                }
            }
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
                AnimBase anim = characters.characters[player.character].animations[player.changebleStats.PlayerAnimation].data;
                float dir = 1;
                int input = player.GetInput().movement;
                if (!player.changebleStats.IsLookingRight)
                {
                    if (input % 3 == 1)
                    {
                        input += 2;
                    }
                    else if (input % 3 == 0)
                    {
                        input -= 2;
                    }
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
                rect = characters.characters[player.character].blockHightBox;
                if (/*input == 4 &&*/ (player.changebleStats.PlayerAnimation == Animations.WalkBack || player.changebleStats.PlayerAnimation == Animations.BlockHigh || player.changebleStats.PlayerAnimation == Animations.BlockLow))
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                rect = characters.characters[player.character].blockLowBox;
                if (/*input == 1 && */(player.changebleStats.PlayerAnimation == Animations.CrouchBlock || player.changebleStats.PlayerAnimation == Animations.BlockHigh || player.changebleStats.PlayerAnimation == Animations.BlockLow))
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
            }
        }
    }
}
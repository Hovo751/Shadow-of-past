using Coherence.Cloud;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
    public const int GetHitAir = 12;
    public const int KnockdownRec = 13;
    public const int BlockHigh = 14;
    public const int BlockLow = 15;
    public const int Light = 16;
    public const int Medium = 17;
    public const int Heavy = 18;
    public const int LightCrouch = 19;
    public const int MediumCrouch = 20;
    public const int HeavyCrouch = 21;
    public const int JumpLight = 22;
    public const int JumpMedium = 23;
    public const int JumpHeavy = 24;
}
public class Simulation : MonoBehaviour
{
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
    public struct MovementAndButtonInput
    {
        public int movement;
        public bool light;
        public bool medium;
        public bool heavy;
    }

    public int juggleLimit = 2;
    public int InputBufferSize;

    public Dictionary<long, SimulationState> history = new Dictionary<long, SimulationState>();
    public long startFrame = -1;
    private long currentFrame = -1;

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

        int overlapLeft = Mathf.Max(aLeft, bLeft);
        int overlapRight = Mathf.Min(aRight, bRight);

        int overlapBottom = Mathf.Max(aBottom, bBottom);
        int overlapTop = Mathf.Min(aTop, bTop);

        centerX = (overlapLeft + overlapRight) / 2;
        centerY = (overlapBottom + overlapTop) / 2;

        return true;
    }
    private bool isInAir(PlayerChangebleStats player)
    {
        if (player.PlayerPositionVertical <= 0)
        {
            return false;
        }
        return true;
    }
    private void PlayAnimation(ref PlayerChangebleStats result, Character character, int anim)
    {
        result.PlayerNextAnimation = character.animations[anim].nextAnim;
        result.PlayerNextAnimationFrame = 0;
        result.HitLanded = 0;
        if (character.animations[anim].data.inAir)
        {
            result.PlayerNextAnimationFrame = result.PlayerAnimationFrame + character.animations[anim].data.frames.Length;
            result.PlayerNextAnimation = result.PlayerAnimation;
        }
        result.PlayerAnimationFrame = 0;
        result.PlayerAnimation = anim;
    }
    private void PlayAnimation(ref PlayerChangebleStats result, Character character, int anim, int frame)
    {
        PlayAnimation(ref result, character, anim);
        result.PlayerAnimationFrame = frame;
        if (character.animations[anim].data.frames.Length <= frame)
            result.PlayerAnimationFrame = character.animations[anim].data.frames.Length - 1;
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
    ButtonInput[] SortInputs(bool[] arr)
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
            if (currentFrame - i > startFrame && startFrame != -1 && i != 0)
            {
                if (history[currentFrame - i].skipFrames > 0)
                {
                    ButtonInput input = movementBuffer[movementBuffer.Count - 1];
                    input.holdTime--;
                    movementBuffer[movementBuffer.Count - 1] = input;
                }
            }
        }

        return movementBuffer.ToArray();
    }
    bool CanCancelInto(PlayerChangebleStats result, Character character, int anim)
    {
        AnimBase currentAnim = character.animations[result.PlayerAnimation].data;
        AnimBase nextAnim = character.animations[anim].data;
        int hitLanded = Mathf.Min(result.HitLanded, currentAnim.frames[result.PlayerAnimationFrame].cancelLvl.Length - 1);
        if (hitLanded != 0) Debug.Log(hitLanded);
        if (result.PlayerAnimation != anim &&
            currentAnim.frames[result.PlayerAnimationFrame].cancelLvl[hitLanded] <= nextAnim.cancelLvl &&
            nextAnim.inAir == isInAir(result))
            return true;
        return false;
    }
    private bool PressedButtonInInputBuffer(ButtonInput[] buffer)
    {
        if (buffer[0].buttonDown == true && buffer[0].holdTime < InputBufferSize) return true;
        if (buffer[0].buttonDown == false && buffer.Length >= 2)
        {
            if (buffer[1].buttonDown == true && buffer[0].holdTime + buffer[1].holdTime < InputBufferSize)
            {
                return true;
            }
        }
        return false;
    }
    private PlayerChangebleStats CalculatePerPlayer(MovementAndButtonInput[] inputs, PlayerChangebleStats result, Character character)
    {
        int movement = inputs[0].movement;
        if (movement > 9 || movement < 1)
        {
            return result;
        }

        int[] movementBufferNotSorted = new int[64];
        bool[] lightButtonBufferNotSorted = new bool[64];
        bool[] mediumButtonBufferNotSorted = new bool[64];
        bool[] heavyButtonBufferNotSorted = new bool[64];

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
        for (long i = 0; i < 64; i++)
        {
            if (i < inputs.Length)
            {
                MovementAndButtonInput inputPast = inputs[i];
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
                movementBufferNotSorted[i] = m;
                lightButtonBufferNotSorted[i] = inputPast.light;
                mediumButtonBufferNotSorted[i] = inputPast.medium;
                heavyButtonBufferNotSorted[i] = inputPast.heavy;
            }
            else
            {
                movementBufferNotSorted[i] = 5;
                lightButtonBufferNotSorted[i] = false;
                mediumButtonBufferNotSorted[i] = false;
                heavyButtonBufferNotSorted[i] = false;
            }
        }

        MovementInput[] movementBuffer = SortInputs(movementBufferNotSorted);
        ButtonInput[] lightButtonBuffer = SortInputs(lightButtonBufferNotSorted);
        ButtonInput[] mediumButtonBuffer = SortInputs(mediumButtonBufferNotSorted);
        ButtonInput[] heavyButtonBuffer = SortInputs(heavyButtonBufferNotSorted);

        result.IsInAir = isInAir(result);

        //Stepping over to next frame of animation and changing it based on the inputs

        AnimBase currentAnimation = character.animations[result.PlayerAnimation].data;

        result.PlayerAnimationFrame++;

        if (result.PlayerAnimation == Animations.GetHitUp || result.PlayerAnimation == Animations.GetHitDown || result.PlayerAnimation == Animations.GetHitAir ||
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
            if (result.PlayerAnimation == Animations.GetHitAir && !isInAir(result))
            {
                result.PlayerAnimation = Animations.KnockdownRec;
                result.PlayerAnimationFrame = 0;
                result.InHitstun = 0;
                result.PlayerPositionVertical = 0;
            }
        }

        currentAnimation = character.animations[result.PlayerAnimation].data;

        if (currentAnimation.inAir && !isInAir(result))
        {
            int jumpAnimLength = character.animations[Animations.JumpStraight].data.frames.Length;
            PlayAnimation(ref result, character, Animations.JumpStraight, jumpAnimLength - 1);
        }

        currentAnimation = character.animations[result.PlayerAnimation].data;
        if (currentAnimation.frames.Length <= result.PlayerAnimationFrame && result.PlayerAnimation != Animations.GetHitUp)
        {
            PlayAnimation(ref result, character, result.PlayerNextAnimation, result.PlayerNextAnimationFrame);
        }
        if (PressedButtonInInputBuffer(heavyButtonBuffer))
        {
            if (CanCancelInto(result, character, Animations.Heavy) && movement != 1 && movement != 2 && movement != 3)
                PlayAnimation(ref result, character, Animations.Heavy);
            else if (CanCancelInto(result, character, Animations.HeavyCrouch) && (movement == 1 || movement == 2 || movement == 3))
                PlayAnimation(ref result, character, Animations.HeavyCrouch);
            else if (CanCancelInto(result, character, Animations.JumpHeavy))
                PlayAnimation(ref result, character, Animations.JumpHeavy);
        }
        else if (PressedButtonInInputBuffer(mediumButtonBuffer))
        {
            if (CanCancelInto(result, character, Animations.Medium) && movement != 1 && movement != 2 && movement != 3)
                PlayAnimation(ref result, character, Animations.Medium);
            else if (CanCancelInto(result, character, Animations.MediumCrouch) && (movement == 1 || movement == 2 || movement == 3))
                PlayAnimation(ref result, character, Animations.MediumCrouch);
            else if (CanCancelInto(result, character, Animations.JumpMedium))
                PlayAnimation(ref result, character, Animations.JumpMedium);
        }
        else if (PressedButtonInInputBuffer(lightButtonBuffer))
        {
            if (CanCancelInto(result, character, Animations.Light) && movement != 1 && movement != 2 && movement != 3)
                PlayAnimation(ref result, character, Animations.Light);
            else if (CanCancelInto(result, character, Animations.LightCrouch) && (movement == 1 || movement == 2 || movement == 3))
                PlayAnimation(ref result, character, Animations.LightCrouch);
            else if (CanCancelInto(result, character, Animations.JumpLight))
                PlayAnimation(ref result, character, Animations.JumpLight);
        }
        if (movement == 4)
        {
            if (movementBuffer.Length > 3 &&
                    movementBuffer[0].holdTime <= 10 &&
                    movementBuffer[1].input == 5 && movementBuffer[1].holdTime <= 10 &&
                    movementBuffer[2].input == 4 && movementBuffer[2].holdTime <= 10 &&
                    CanCancelInto(result, character, Animations.DashBackward))
            {
                PlayAnimation(ref result, character, Animations.DashBackward);
            }
            else if (CanCancelInto(result, character, Animations.WalkBack))
            {

                PlayAnimation(ref result, character, Animations.WalkBack);
            }
        }
        else if (movement == 6)
        {
            if (movementBuffer.Length > 3 &&
                    movementBuffer[0].holdTime <= 10 &&
                    movementBuffer[1].input == 5 && movementBuffer[1].holdTime <= 10 &&
                    movementBuffer[2].input == 6 && movementBuffer[2].holdTime <= 10 &&
                    CanCancelInto(result, character, Animations.DashForward))
            {
                PlayAnimation(ref result, character, Animations.DashForward);
            }
            else if (CanCancelInto(result, character, Animations.WalkForward))
            {

                PlayAnimation(ref result, character, Animations.WalkForward);
            }
        }
        else if (CanCancelInto(result, character, Animations.Crouch) && (movement == 2 || movement == 3))
        {
            PlayAnimation(ref result, character, Animations.Crouch);
        }
        else if (CanCancelInto(result, character, Animations.CrouchBlock) && movement == 1)
        {
            PlayAnimation(ref result, character, Animations.CrouchBlock);
        }
        else if (CanCancelInto(result, character, Animations.JumpStraight) && movement == 8)
        {
            PlayAnimation(ref result, character, Animations.JumpStraight);
        }
        else if (CanCancelInto(result, character, Animations.JumpForward) && movement == 9)
        {
            PlayAnimation(ref result, character, Animations.JumpForward);
        }
        else if (CanCancelInto(result, character, Animations.JumpBack) && movement == 7)
        {
            PlayAnimation(ref result, character, Animations.JumpBack);
        }
        else if (CanCancelInto(result, character, Animations.Idle) && movement == 5)
        {
            PlayAnimation(ref result, character, Animations.Idle);
        }

        currentAnimation = character.animations[result.PlayerAnimation].data;

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

    public SimulationState Simulate(SimulationState state, MovementAndButtonInput[] player1Inputs, MovementAndButtonInput[] player2Inputs, Character player1Character, Character player2Character, long simulationFrame)
    {
        int skipFrames = state.skipFrames;
        currentFrame = simulationFrame;
        if (skipFrames > 0)
        {
            skipFrames--;
            state.skipFrames = skipFrames;
            return state;
        }

        int player1MovementInput = player1Inputs[0].movement;
        int player2MovementInput = player2Inputs[0].movement;

        //Calculating the players position based on velocity and acceleration and also playing animations based on players inputs

        PlayerChangebleStats player1Data = CalculatePerPlayer(player1Inputs, state.PlayerData[0], player1Character);
        PlayerChangebleStats player2Data = CalculatePerPlayer(player2Inputs, state.PlayerData[1], player2Character);

        //Setting which way the players look

        Frame player1Frame = player1Character.animations[player1Data.PlayerAnimation].data.frames[player1Data.PlayerAnimationFrame];
        Frame player2Frame = player2Character.animations[player2Data.PlayerAnimation].data.frames[player2Data.PlayerAnimationFrame];

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
        int midle;
        int verticalMidle;
        if (CheckCollision(player1Collision, player2Collision, out midle, out verticalMidle))
        {
            Rectengale player1CollisionCopy = player1Frame.collisionBox;
            Rectengale player2CollisionCopy = player2Frame.collisionBox;
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

                    if (isInAir(player2Data) && player2Data.juggleScaling > juggleLimit)
                    {
                        break;
                    }

                    if (CheckCollision(player1Collision, player2Collision, out player2HitX, out player2HitY))
                    {
                        int isBlocking = -1;

                        if (player2MovementInput == 4 &&
                            (player2Data.PlayerAnimation == Animations.WalkBack ||
                            player2Data.PlayerAnimation == Animations.BlockHigh ||
                            player2Data.PlayerAnimation == Animations.BlockLow))
                        {
                            player2Collision = player2Character.blockHightBox;
                            isBlocking = Animations.BlockHigh;
                        }
                        else if (player2MovementInput == 1 &&
                            (player2Data.PlayerAnimation == Animations.CrouchBlock ||
                            player2Data.PlayerAnimation == Animations.BlockHigh ||
                            player2Data.PlayerAnimation == Animations.BlockLow))
                        {
                            player2Collision = player2Character.blockLowBox;
                            isBlocking = Animations.BlockLow;
                        }

                        player2Collision.posX *= player2Dir;
                        player2Collision.posX += player2Data.PlayerPositionHorizontal;
                        player2Collision.posY += player2Data.PlayerPositionVertical;

                        int didBlock = -1;

                        if (isBlocking != -1 && (CheckCollision(player1Collision, player2Collision) || !player1Frame.canHitHighOrLow))
                        {
                            didBlock = isBlocking;
                        }

                        if (player1Collision.posY > (player2Character.blockHightBox.posY + player2Character.blockLowBox.posY) / 2 && didBlock == -1)
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
                        if (didBlock == -1 && isInAir(player2Data))
                        {
                            player2Gothit = Animations.GetHitAir;
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

                    if (isInAir(player1Data) && player1Data.juggleScaling > juggleLimit)
                    {
                        break;
                    }

                    if (CheckCollision(player2Collision, player1Collision, out player1HitX, out player1HitY))
                    {
                        int isBlocking = -1;

                        if (player1MovementInput == 4 &&
                            (player1Data.PlayerAnimation == Animations.WalkBack ||
                             player1Data.PlayerAnimation == Animations.BlockHigh ||
                             player1Data.PlayerAnimation == Animations.BlockLow))
                        {
                            player1Collision = player1Character.blockHightBox;
                            isBlocking = Animations.BlockHigh;
                        }
                        else if (player1MovementInput == 1 &&
                            (player1Data.PlayerAnimation == Animations.CrouchBlock ||
                             player1Data.PlayerAnimation == Animations.BlockHigh ||
                             player1Data.PlayerAnimation == Animations.BlockLow))
                        {
                            player1Collision = player1Character.blockLowBox;
                            isBlocking = Animations.BlockLow;
                        }

                        player1Collision.posX *= player1Dir;
                        player1Collision.posX += player1Data.PlayerPositionHorizontal;
                        player1Collision.posY += player1Data.PlayerPositionVertical;

                        int didBlock = -1;

                        if (isBlocking != -1 && (CheckCollision(player2Collision, player1Collision) || !player2Frame.canHitHighOrLow))
                        {
                            didBlock = isBlocking;
                        }

                        if (player2Collision.posY > (player1Character.blockHightBox.posY + player1Character.blockLowBox.posY) / 2 && didBlock == -1)
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
                        if (didBlock == -1 && isInAir(player1Data))
                        {
                            player1Gothit = Animations.GetHitAir;
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

        if (!isInAir(player1Data))
        {
            player1Data.juggleScaling = 0;
        }
        if (!isInAir(player2Data))
        {
            player2Data.juggleScaling = 0;
        }

        //Apply the required data if pl1 or pl2 got hit

        if (player1Gothit != -1)
        {
            if (player1Gothit == Animations.BlockHigh || player1Gothit == Animations.BlockLow)
            {
                player1Data.InHitstun = player2Frame.hitboxBlockStun;
                player1Data.Health -= player2Frame.chipDamage;
                player1Data.PlayerAnimation = player1Gothit;
                player1Data.PlayerAnimationFrame = 0;
                player1Data.PlayerNextAnimation = Animations.Idle;
                player1Data.PlayerNextAnimationFrame = 0;
                if (player1Data.PlayerPositionHorizontal <= -9990 || player1Data.PlayerPositionHorizontal >= 9990)
                {
                    if (!isInAir(player2Data))
                    {
                        player2Data.PlayerVelocityHorizontal = -player2Frame.hitboxPushOnBlock * player2Dir;
                        player2Data.PlayerAccelerationHorizontal = player2Data.PlayerVelocityHorizontal / -12;
                    }
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
                state.player2LandedHit = true;
                player2Data.Combo++;
                player1Data.InHitstun = player2Frame.hitboxHitStun;
                player1Data.Health -= player2Frame.hitboxDamage;
                if (player1Gothit == Animations.GetHitAir) player1Data.InHitstun = 999999;
                if (player1Gothit == Animations.GetHitAir && player2Data.Combo != 0) player1Data.juggleScaling += player2Frame.addJuggleScaling;
                player1Data.PlayerAnimation = player1Gothit;
                player1Data.PlayerAnimationFrame = 0;
                player1Data.PlayerNextAnimation = Animations.Idle;
                player1Data.PlayerNextAnimationFrame = 0;
                if (player1Data.PlayerPositionHorizontal <= -9990 || player1Data.PlayerPositionHorizontal >= 9990)
                {
                    if (!isInAir(player2Data))
                    {
                        player2Data.PlayerVelocityHorizontal = -player2Frame.hitboxPush * player2Dir;
                        player2Data.PlayerAccelerationHorizontal = player2Data.PlayerVelocityHorizontal / -12;
                    }
                }
                else
                {
                    player1Data.PlayerVelocityHorizontal = player2Frame.hitboxPush * player2Dir;
                    player1Data.PlayerAccelerationHorizontal = player1Data.PlayerVelocityHorizontal / -12;
                    if (player1Gothit == Animations.GetHitAir)
                    {
                        player1Data.PlayerVelocityHorizontal = player2Frame.hitboxPushAirHorizontal * player2Dir;
                        player1Data.PlayerAccelerationHorizontal = 0;
                    }
                }
                if (player1Gothit == Animations.GetHitAir) player1Data.PlayerVelocityVertical = player2Frame.hitboxPushAirVertical;
                player2Data.HitLanded = player2Frame.hitboxLand;
                skipFrames = player2Frame.hitStop;
            }
        }

        if (player2Gothit != -1)
        {
            if (player2Gothit == Animations.BlockHigh || player2Gothit == Animations.BlockLow)
            {
                player2Data.InHitstun = player1Frame.hitboxBlockStun;
                player2Data.Health -= player1Frame.chipDamage;
                player2Data.PlayerAnimation = player2Gothit;
                player2Data.PlayerAnimationFrame = 0;
                player2Data.PlayerNextAnimation = Animations.Idle;
                player2Data.PlayerNextAnimationFrame = 0;
                if (player2Data.PlayerPositionHorizontal <= -9990 || player2Data.PlayerPositionHorizontal >= 9990)
                {

                    if (!isInAir(player1Data))
                    {
                        player1Data.PlayerVelocityHorizontal = -player1Frame.hitboxPushOnBlock * player1Dir;
                        player1Data.PlayerAccelerationHorizontal = player1Data.PlayerVelocityHorizontal / -12;
                    }
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
                state.player1LandedHit = true;
                player1Data.Combo++;
                player2Data.InHitstun = player1Frame.hitboxHitStun;
                player2Data.Health -= player1Frame.hitboxDamage;
                if (player2Gothit == Animations.GetHitAir) player2Data.InHitstun = 999999;
                if (player2Gothit == Animations.GetHitAir && player1Data.Combo != 0) player2Data.juggleScaling += player1Frame.addJuggleScaling;
                player2Data.PlayerAnimation = player2Gothit;
                player2Data.PlayerAnimationFrame = 0;
                player2Data.PlayerNextAnimation = Animations.Idle;
                player2Data.PlayerNextAnimationFrame = 0;
                if (player2Data.PlayerPositionHorizontal <= -9990 || player2Data.PlayerPositionHorizontal >= 9990)
                {
                    if (!isInAir(player1Data))
                    {
                        player1Data.PlayerVelocityHorizontal = -player1Frame.hitboxPush * player1Dir;
                        player1Data.PlayerAccelerationHorizontal = player1Data.PlayerVelocityHorizontal / -12;
                    }
                }
                else
                {
                    player2Data.PlayerVelocityHorizontal = player1Frame.hitboxPush * player1Dir;
                    player2Data.PlayerAccelerationHorizontal = player2Data.PlayerVelocityHorizontal / -12;
                    if (player2Gothit == Animations.GetHitAir)
                    {
                        player2Data.PlayerVelocityHorizontal = player1Frame.hitboxPushAirHorizontal * player1Dir;
                        player2Data.PlayerAccelerationHorizontal = 0;
                    }
                }
                if (player2Gothit == Animations.GetHitAir) player2Data.PlayerVelocityVertical = player1Frame.hitboxPushAirVertical;
                player1Data.HitLanded = player1Frame.hitboxLand;
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
        state.PlayerData[0] = player1Data;
        state.PlayerData[1] = player2Data;
        state.skipFrames = skipFrames;
        return state;
    }
}

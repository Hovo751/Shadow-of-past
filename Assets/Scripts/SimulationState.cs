using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public struct SimulationState
{
    public PlayerChangebleStats[] PlayerData;
}
public struct MovementInput
{
    public int input;
    public int holdTime;
}
public struct FrameInput
{
    public int movement;
}
public class FrameBuffer
{
    public LinkedList<MovementInput> movementInputs = new LinkedList<MovementInput>();
    const int maxSize = 64;
    int size = 0;
    public void addInput( FrameInput input )
    {
        if (movementInputs == null)
        {
            movementInputs = new LinkedList<MovementInput>();
        }
        size++;
        MovementInput inputCopy = new MovementInput();
        if (movementInputs.Count > 0)
        {
            if (movementInputs.First.Value.input == input.movement)
            {
                inputCopy = movementInputs.First.Value;
                inputCopy.holdTime++;
                movementInputs.First.Value = inputCopy;
            }
            else
            {
                inputCopy.input = input.movement;
                inputCopy.holdTime = 1;
                movementInputs.AddFirst(inputCopy);
            }
            if (size > maxSize)
            {
                inputCopy = movementInputs.Last.Value;
                inputCopy.holdTime--;
                if (inputCopy.holdTime <= 0)
                {
                    movementInputs.RemoveLast();
                }
                else
                {
                    movementInputs.Last.Value = inputCopy;
                }
            }
        }
        else
        {
            inputCopy.input = input.movement;
            inputCopy.holdTime = 1;
            movementInputs.AddFirst(inputCopy);
        }
    }
}
[System.Serializable]
public struct PlayerChangebleStats
{
    public int PlayerPositionHorizontal;
    public int PlayerPositionVertical;
    public int PlayerVelocityHorizontal;
    public int PlayerVelocityVertical;
    public int PlayerAccelerationHorizontal;
    public int PlayerAccelerationVertical;
    public int PlayerAnimation;
    public int PlayerNextAnimation;
    public int PlayerAnimationFrame;
    public int HitLanded;
    public int InHitstun;
    public bool IsLookingRight;
    public bool IsInAir;
    public FrameBuffer buffer;

    public static PlayerChangebleStats operator +(PlayerChangebleStats x, PlayerChangebleStats y)
    {
        return new PlayerChangebleStats
        {
            PlayerPositionHorizontal = x.PlayerPositionHorizontal + y.PlayerPositionHorizontal,
            PlayerPositionVertical = x.PlayerPositionVertical + y.PlayerPositionVertical,
            PlayerVelocityHorizontal = x.PlayerVelocityHorizontal + y.PlayerVelocityHorizontal,
            PlayerVelocityVertical = x.PlayerVelocityVertical + y.PlayerVelocityVertical,
            PlayerAccelerationHorizontal = x.PlayerAccelerationHorizontal + y.PlayerAccelerationHorizontal,
            PlayerAccelerationVertical = x.PlayerAccelerationVertical + y.PlayerAccelerationVertical,
            PlayerAnimation = x.PlayerAnimation + y.PlayerAnimation,
            PlayerNextAnimation = x.PlayerNextAnimation + y.PlayerNextAnimation,
            PlayerAnimationFrame = x.PlayerAnimationFrame + y.PlayerAnimationFrame,
            HitLanded = x.HitLanded + y.HitLanded,
            InHitstun = x.InHitstun + y.InHitstun
        };
    }

    public static PlayerChangebleStats operator -(PlayerChangebleStats x, PlayerChangebleStats y)
    {
        return new PlayerChangebleStats
        {
            PlayerPositionHorizontal = x.PlayerPositionHorizontal - y.PlayerPositionHorizontal,
            PlayerPositionVertical = x.PlayerPositionVertical - y.PlayerPositionVertical,
            PlayerVelocityHorizontal = x.PlayerVelocityHorizontal - y.PlayerVelocityHorizontal,
            PlayerVelocityVertical = x.PlayerVelocityVertical - y.PlayerVelocityVertical,
            PlayerAccelerationHorizontal = x.PlayerAccelerationHorizontal - y.PlayerAccelerationHorizontal,
            PlayerAccelerationVertical = x.PlayerAccelerationVertical - y.PlayerAccelerationVertical,
            PlayerAnimation = x.PlayerAnimation - y.PlayerAnimation,
            PlayerNextAnimation = x.PlayerNextAnimation - y.PlayerNextAnimation,
            PlayerAnimationFrame = x.PlayerAnimationFrame - y.PlayerAnimationFrame,
            HitLanded = x.HitLanded - y.HitLanded,
            InHitstun = x.InHitstun - y.InHitstun
        };
    }
}
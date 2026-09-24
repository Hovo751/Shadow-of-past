using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public struct SimulationState
{
    public PlayerChangebleStats[] PlayerData;
    public int skipFrames;
    public Vector2Int player1LandedHit;
    public Vector2Int player2LandedHit;
}
[System.Serializable]
public struct PlayerChangebleStats
{
    public int Health;
    public int PlayerPositionHorizontal;
    public int PlayerPositionVertical;
    public int PlayerVelocityHorizontal;
    public int PlayerVelocityVertical;
    public int PlayerAccelerationHorizontal;
    public int PlayerAccelerationVertical;
    public int PlayerAnimation;
    public int PlayerNextAnimation;
    public int PlayerAnimationFrame;
    public int PlayerNextAnimationFrame;
    public int HitLanded;
    public int InHitstun;
    public bool IsLookingRight;
    public bool IsInAir;
    public int Combo;
    public int juggleScaling;

    //public static PlayerChangebleStats operator +(PlayerChangebleStats x, PlayerChangebleStats y)
    //{
    //    return new PlayerChangebleStats
    //    {
    //        PlayerPositionHorizontal = x.PlayerPositionHorizontal + y.PlayerPositionHorizontal,
    //        PlayerPositionVertical = x.PlayerPositionVertical + y.PlayerPositionVertical,
    //        PlayerVelocityHorizontal = x.PlayerVelocityHorizontal + y.PlayerVelocityHorizontal,
    //        PlayerVelocityVertical = x.PlayerVelocityVertical + y.PlayerVelocityVertical,
    //        PlayerAccelerationHorizontal = x.PlayerAccelerationHorizontal + y.PlayerAccelerationHorizontal,
    //        PlayerAccelerationVertical = x.PlayerAccelerationVertical + y.PlayerAccelerationVertical,
    //        PlayerAnimation = x.PlayerAnimation + y.PlayerAnimation,
    //        PlayerNextAnimation = x.PlayerNextAnimation + y.PlayerNextAnimation,
    //        PlayerAnimationFrame = x.PlayerAnimationFrame + y.PlayerAnimationFrame,
    //        HitLanded = x.HitLanded + y.HitLanded,
    //        InHitstun = x.InHitstun + y.InHitstun
    //    };
    //}

    //public static PlayerChangebleStats operator -(PlayerChangebleStats x, PlayerChangebleStats y)
    //{
    //    return new PlayerChangebleStats
    //    {
    //        PlayerPositionHorizontal = x.PlayerPositionHorizontal - y.PlayerPositionHorizontal,
    //        PlayerPositionVertical = x.PlayerPositionVertical - y.PlayerPositionVertical,
    //        PlayerVelocityHorizontal = x.PlayerVelocityHorizontal - y.PlayerVelocityHorizontal,
    //        PlayerVelocityVertical = x.PlayerVelocityVertical - y.PlayerVelocityVertical,
    //        PlayerAccelerationHorizontal = x.PlayerAccelerationHorizontal - y.PlayerAccelerationHorizontal,
    //        PlayerAccelerationVertical = x.PlayerAccelerationVertical - y.PlayerAccelerationVertical,
    //        PlayerAnimation = x.PlayerAnimation - y.PlayerAnimation,
    //        PlayerNextAnimation = x.PlayerNextAnimation - y.PlayerNextAnimation,
    //        PlayerAnimationFrame = x.PlayerAnimationFrame - y.PlayerAnimationFrame,
    //        HitLanded = x.HitLanded - y.HitLanded,
    //        InHitstun = x.InHitstun - y.InHitstun
    //    };
    //}
}
using UnityEngine;

public struct SimulationState
{
    public PlayerChangebleStats[] PlayerData;
}

public struct PlayerChangebleStats
{
    public int PlayerPositionHorizontal;
    public int PlayerPositionVertical;
    public int PlayerVelocityHorizontal;
    public int PlayerVelocityVertical;
    public int PlayerAnimation;
    public int PlayerNextAnimation;
    public int PlayerAnimationFrame;

    public static PlayerChangebleStats operator +(PlayerChangebleStats x, PlayerChangebleStats y)
    {
        return new PlayerChangebleStats
        {
            PlayerPositionHorizontal = x.PlayerPositionHorizontal + y.PlayerPositionHorizontal,
            PlayerPositionVertical = x.PlayerPositionVertical + y.PlayerPositionVertical,
            PlayerVelocityHorizontal = x.PlayerVelocityHorizontal + y.PlayerVelocityHorizontal,
            PlayerVelocityVertical = x.PlayerVelocityVertical + y.PlayerVelocityVertical,
            PlayerAnimation = x.PlayerAnimation + y.PlayerAnimation,
            PlayerNextAnimation = x.PlayerNextAnimation + y.PlayerNextAnimation,
            PlayerAnimationFrame = x.PlayerAnimationFrame + y.PlayerAnimationFrame
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
            PlayerAnimation = x.PlayerAnimation - y.PlayerAnimation,
            PlayerNextAnimation = x.PlayerNextAnimation - y.PlayerNextAnimation,
            PlayerAnimationFrame = x.PlayerAnimationFrame - y.PlayerAnimationFrame
        };
    }
}
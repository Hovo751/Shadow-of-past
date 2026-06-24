using UnityEngine;
[System.Serializable]
public struct Rectengale
{
    public int posX;
    public int posY;
    public int sizeX;
    public int sizeY;
}
[System.Serializable]
public struct Frame
{
    public int cancelLvl;
    public int addPosX;
    public int addPosY;
    public int setVelocityHorizontal;
    public bool setVelocityHbool;
    public int setVelocityVertical;
    public bool setVelocityVbool;
    public int setAccelerationHorizontal;
    public bool setAccelerationHbool;
    public int setAccelerationVertical;
    public bool setAccelerationVbool;
    public Rectengale[] hurtbox;
    public int hitboxPush;
    public int hitboxPushOnBlock;
    public int hitboxLand;
    public int hitboxDamage;
    public int hitboxHitStun;
    public int hitStop;
    public int hitStopOnBlock;
    public int hitboxBlockStun;
    public bool canHitHighOrLow;
    public bool canRotate;
    public Rectengale[] hitbox;
    public Rectengale[] throwbox;
    public Rectengale collisionBox;
}
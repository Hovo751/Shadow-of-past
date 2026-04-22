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
    public Rectengale[] hurtbox;
    public int hurtboxHit;
    public int hurtboxDamage;
    public int hurtboxHitStun;
    public int hurtboxBlockStun;
    public Rectengale[] hitbox;
    public Rectengale[] blockbox;
    public Rectengale[] collisionBox;
}
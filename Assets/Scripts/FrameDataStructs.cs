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
    public int addPosX;
    public int addPosY;
    public Rectengale[] hurtbox;
    public int hurtboxHit;
    public Rectengale[] hitbox;
    public Rectengale[] throwbox;
    public Rectengale[] collisionBox;
}
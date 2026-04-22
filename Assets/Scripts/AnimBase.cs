using UnityEngine;

[CreateAssetMenu(fileName = "AnimBase", menuName = "Scriptable Objects/AnimBase")]
public class AnimBase : ScriptableObject
{
    public int cancelLvl;
    public bool inAir;
    public Frame[] frames;
}
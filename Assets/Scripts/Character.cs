using UnityEngine;
using static Simulation;

[CreateAssetMenu(fileName = "Character", menuName = "Scriptable Objects/Character")]
public class Character : ScriptableObject
{
    [System.Serializable]
    public struct AnimationData
    {
        public string name;
        public AnimBase data;
        public int nextAnim;
    }
    public AnimationData[] animations;
    public Rectengale blockHightBox; 
    public Rectengale blockLowBox;
}

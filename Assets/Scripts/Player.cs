using Coherence.Toolkit;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CoherenceSync))]
[RequireComponent(typeof(CoherenceInput))]
public class Player : MonoBehaviour
{
    public int character = 0;
    public int Speed = 50;
    public int JumpPower = 100;
    public int JumpPowerSide = 50;
    public int Gravity = 10;

    //public int horizontalPos = -5000;
    //public int horizontalVelocity = 0;
    //public int verticalPos = 0;
    //public int verticalVelocity = 0;
    //public int animationType = 0;
    //public int nextAnimation = 0;
    //public int animationFrame = 0;

    public PlayerChangebleStats changebleStats;

    private CoherenceInput input;
    private void Update()
    {
        transform.position = new Vector3(changebleStats.PlayerPositionHorizontal / 1000.0f, changebleStats.PlayerPositionVertical / 1000.0f, 0);
    }

    private void Awake()
    {
        input = GetComponent<CoherenceInput>();
    }

    // Retrieves the "movement" input state for a given frame
    public int GetMovement(long frame)
    {
        return input.GetInteger("Move", frame);
    }

    // Sets the "movement" state for the current frame
    public void SetMovement()
    {
        int movement = 5 + (int)Input.GetAxis("Horizontal") + (int)Input.GetAxis("Vertical") * 3;
        input.SetInteger("Move", movement);
    }
}
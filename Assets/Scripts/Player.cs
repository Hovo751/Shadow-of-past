using Coherence.Toolkit;
using TMPro;
using UnityEngine;
using static System.TimeZoneInfo;

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
    public Animator animator;
    public Characters characters;
    public float x;

    private CoherenceInput input;
    private float transitionDuration = 1.0f;
    private string previousAnim;
    private float prevT;
    private string previousAnimSaved;
    private float prevTSaved;

    public float transitionTime = 0.2f;
    private void Update()
    {
        transform.position = new Vector3(changebleStats.PlayerPositionHorizontal / 1000.0f, changebleStats.PlayerPositionVertical / 1000.0f + x, 0);
        if (changebleStats.IsLookingRight)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, -90, 0);
        }
        transitionDuration += Time.deltaTime;
        int animId = changebleStats.PlayerAnimation;
        if (animId == 7) animId = 6;
        string animationName = characters.characters[character].data[animId].name;
        float t = ((float) changebleStats.PlayerAnimationFrame) / characters.characters[character].data[changebleStats.PlayerAnimation].data.frames.Length;
        animator.Play(animationName, 0, t);

        if (animationName != previousAnim)
        {
            previousAnimSaved = previousAnim;
            prevTSaved = prevT;
            transitionDuration = 0;
        }

        // previous anim (layer 1)
        animator.Play(previousAnimSaved, 1, prevTSaved);

        // blend weight
        float blend = Mathf.Clamp01(transitionDuration / transitionTime);
        animator.SetLayerWeight(1, 1f - blend);

        animator.Update(0f);

        previousAnim = animationName;
        prevT = t;
    }

    private void Awake()
    {
        input = GetComponent<CoherenceInput>();
        animator.speed = 0.0f;
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
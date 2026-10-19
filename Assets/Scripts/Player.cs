using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int character = 0;

    //public int horizontalPos = -5000;
    //public int horizontalVelocity = 0;
    //public int verticalPos = 0;
    //public int verticalVelocity = 0;
    //public int animationType = 0;
    //public int nextAnimation = 0;
    //public int animationFrame = 0;

    public PlayerChangebleStats changebleStats;
    public Animator animator;
    public Simulation simulation;
    public Characters characters;
    public float x;
    public long startFrame = -1;
    public GameObject hitParticle;
    public bool drawParticles = true;
    public long currentFrame = 0;
    
    private AudioSource punchSound;
    private float transitionDuration = 1.0f;
    private string previousAnim;
    private float prevT;
    private string previousAnimSaved;
    private float prevTSaved;
    private List<GameObject> particleGameobjects = new List<GameObject>();

    public float transitionTime = 0.2f;
    private void Update()
    {
        transform.position = new Vector3(changebleStats.PlayerPosition.x / 1000.0f, changebleStats.PlayerPosition.y / 1000.0f + x, 0);
        transform.rotation = Quaternion.Euler(0, 90, 0);
        if (changebleStats.IsLookingRight)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.localScale = new Vector3(1, 1, -1);
        }
        int animId = changebleStats.PlayerAnimation;
        if (animId == 7) animId = 6;
        string animationName = characters.characters[character].animations[animId].name;
        float t = ((float)changebleStats.PlayerAnimationFrame) / characters.characters[character].animations[changebleStats.PlayerAnimation].data.frames.Length;
        animator.Play(animationName, 0, t);

        if (animationName != previousAnim)
        {
            previousAnimSaved = previousAnim;
            prevTSaved = prevT;
            transitionDuration = 0;
            if (animationName == "BlockHigh" || animationName == "BlockLow")
            {
                transitionDuration = 100000;
            }
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

    public void UpdateParticles()
    {
        if (drawParticles)
        {
            List<Vector3> particles = new List<Vector3>();
            for (int i = 0; i < particleGameobjects.Count; i++)
            {
                Destroy(particleGameobjects[i]);
            }
            particleGameobjects = new List<GameObject>();

            for (int i = 1; i < 60; i++)
            {
                SimulationState s = simulation.GetFrameState(currentFrame - i);
                if (s.player1LandedHit != new Vector2Int(-100000, -100000))
                {
                    particles.Add(new Vector3(s.player1LandedHit.x / 1000.0f, s.player1LandedHit.y / 1000.0f, i / 60.0f));
                }
                if (s.player2LandedHit != new Vector2Int(-100000, -100000))
                {
                    particles.Add(new Vector3(s.player2LandedHit.x / 1000.0f, s.player2LandedHit.y / 1000.0f, i / 60.0f));
                }
            }

            for (int i = 0; i < particles.Count; i++)
            {
                GameObject newParticle = Instantiate(hitParticle);
                newParticle.transform.position = new Vector3(particles[i].x, particles[i].y + x, -8);
                ParticleSystem particleSystem = newParticle.GetComponent<ParticleSystem>();
                particleSystem.useAutoRandomSeed = false;
                particleSystem.randomSeed = 0;
                particleSystem.Simulate(particles[i].z);
                particleGameobjects.Add(newParticle);
            }
        }
    }

    private void Awake()
    {
        animator.speed = 0.0f;
        punchSound = FindAnyObjectByType<AudioSource>();
    }
    // Retrieves the "movement" input state for a given frame
    //public MovementAndButtonInput GetInput(long frame)
    //{
    //    MovementAndButtonInput a;
    //    a.movement = input.GetInteger("Move", frame);
    //    a.light = input.GetButton("Light", frame);
    //    a.medium = input.GetButton("Medium", frame);
    //    a.heavy = input.GetButton("Heavy", frame);
    //    lastFrame = frame;
    //    return a;
    //}

    //public MovementAndButtonInput GetInput()
    //{
    //    MovementAndButtonInput a;
    //    if (lastFrame == -1)
    //    {
    //        a.movement = 0;
    //        a.light = false;
    //        a.medium = false;
    //        a.heavy = false;
    //        return a;
    //    }
    //    a.movement = input.GetInteger("Move", lastFrame);
    //    a.light = input.GetButton("Light", lastFrame);
    //    a.medium = input.GetButton("Medium", lastFrame);
    //    a.heavy = input.GetButton("Heavy", lastFrame);
    //    return a;
    //}

    //// Sets the "movement" state for the current frame
    //public void SetInput()
    //{
    //    int movement = 5 + (int)Input.GetAxis("Horizontal") + (int)Input.GetAxis("Vertical") * 3;
    //    //if ((int)Input.GetAxis("Horizontal") == 0 && (int)Input.GetAxis("Vertical") == 0) movement = 3;
    //    input.SetInteger("Move", movement);
    //    bool light = Input.GetButton("Fire1");
    //    bool medium = Input.GetButton("Fire2");
    //    bool heavy = Input.GetButton("Fire3");
    //    input.SetButton("Light", light);
    //    input.SetButton("Medium", medium);
    //    input.SetButton("Heavy", heavy);
    //}
    public void PlayHit(Vector3 pos)
    {
        pos.x /= 1000f;
        pos.y /= 1000f;
        pos.y += x;
        pos.z = -10.0f;
        GameObject newParticle =  Instantiate(hitParticle);
        newParticle.transform.position = pos;
        ParticleSystem particleSystem = newParticle.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            //particleSystem.Emit(30);
        }
        if (punchSound != null)
        {
            punchSound.Play();
        }
    }
}
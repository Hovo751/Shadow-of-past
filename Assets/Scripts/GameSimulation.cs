using UnityEngine;
using static SendReceive;
using static Memory;

public class GameSimulation : MonoBehaviour
{
    public InputBuffer inputBuffer;
    public Memory memory;
    public GameObject p1;
    public GameObject p2;

    public PlayerData p1Data;
    public PlayerData p2Data;

    private int direction = -1;

    //delete when adding actual speed
    public int speed = 1;

    public int frame = 0;

    private int realFrame = 0;
    private void Start()
    {
        p1Data = new PlayerData();
        p1Data.playerPos = -3000;
        p2Data = new PlayerData();
        p2Data.playerPos = 3000;
        Simulate();
    }
    private void FixedUpdate()
    {
        frame++;
    }
    private void Update()
    {
        Simulate();
    }

    public void LoadFrame(int frameToLoad)
    {
        if (frameToLoad < 0) frameToLoad = 0;
        realFrame = frameToLoad;
        p1Data = memory.player1Data.Get(frameToLoad);
        p2Data = memory.player2Data.Get(frameToLoad);
    }

    private void SaveFrame()
    {
        memory.player1Data.Set(p1Data, realFrame);
        memory.player2Data.Set(p2Data, realFrame);
    }

    private void Render()
    {
        p1.transform.position = new Vector3(p1Data.playerPos / 1000.0f, p1.transform.position.y, p1.transform.position.z);
        p2.transform.position = new Vector3(p2Data.playerPos / 1000.0f, p2.transform.position.y, p2.transform.position.z);
    }

    public void Simulate()
    {
        if (realFrame >= frame)
        {
            return;
        }
        if (p1Data == null)
        {
            p1Data = new PlayerData();
            p1Data.playerPos = -3000;
        }
        if (p2Data == null)
        {
            p2Data = new PlayerData();
            p2Data.playerPos = 3000;
        }
        if (p1Data.playerPos < p2Data.playerPos) direction = 1;
        if (p1Data.playerPos > p2Data.playerPos) direction = -1;

        InputFrame p1Input = inputBuffer.p1inputs.Get(realFrame);
        InputFrame p2Input = inputBuffer.p2inputs.Get(realFrame);

        if (p1Input.moveDir == 6)
        {
            p1Data.playerPos += 1 * direction * speed;
        }
        else if (p1Input.moveDir == 4)
        {
            p1Data.playerPos += -1 * direction * speed;
        }

        if (p2Input.moveDir == 6)
        {
            p2Data.playerPos += -1 * direction * speed;
        }
        else if (p2Input.moveDir == 4)
        {
            p2Data.playerPos += 1 * direction * speed;
        }

        SaveFrame();
        realFrame++;
        if (realFrame >= frame)
        {
            Render();
            return;
        }
        Simulate();
    }
}

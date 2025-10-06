using UnityEngine;

public class GameSimulation : MonoBehaviour
{
    public GameObject p1;
    public GameObject p2;
    //put in input buffer script
    public int p1Input = 0;
    public int p2Input = 0;

    public int p1PosX = -3000;
    public int p2PosX = 3000;

    public int direction = -1;

    //delete when adding actual speed
    public int speed = 1;
    public int frame = 0;
    private void FixedUpdate()
    {
        if (p1PosX < p2PosX) direction = 1;
        if (p1PosX > p2PosX) direction = -1;

        if (p1Input == 6)
        {
            p1PosX += 1 * direction * speed;
        }
        else if (p1Input == 4)
        {
            p1PosX += -1 * direction * speed;
        }

        if (p2Input == 6)
        {
            p2PosX += -1 * direction * speed;
        }
        else if (p2Input == 4)
        {
            p2PosX += 1 * direction * speed;
        }

        p1.transform.position = new Vector3(p1PosX / 1000.0f, p1.transform.position.y, p1.transform.position.z);
        p2.transform.position = new Vector3(p2PosX / 1000.0f, p2.transform.position.y, p2.transform.position.z);
        frame++;
    }
}

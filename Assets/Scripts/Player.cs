using Coherence.Toolkit;
using UnityEngine;

[RequireComponent(typeof(CoherenceSync))]
[RequireComponent(typeof(CoherenceInput))]
public class Player : MonoBehaviour
{
    // Movement speed in units/sec
    public float Speed = 5f;
    public int horizontalPos = -5000;
    public int verticalPos = 0;

    private CoherenceInput input;
    private void Update()
    {
        transform.position = new Vector3(horizontalPos / 1000.0f, verticalPos / 1000.0f, 0);
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
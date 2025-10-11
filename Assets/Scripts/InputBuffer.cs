using UnityEngine;
using static SendReceive;

public class InputBuffer : MonoBehaviour
{
    public GameSimulation simulation;
    public class InputBufferArray
    {
        private InputFrame[] buffer;
        private int size;
        private GameSimulation simulation;

        public InputBufferArray(int size, GameSimulation sim)
        {
            this.size = size;
            buffer = new InputFrame[size];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i].frame = -1;
                buffer[i].moveDir = -1;
            }

            this.simulation = sim;
        }

        public void Set(InputFrame input, bool rollback)
        {
            buffer[input.frame % size] = input;
            if (rollback)
            {
                if (input.frame > simulation.frame)
                {
                    simulation.frame++;
                }
                else if (input.frame < simulation.frame)
                {
                    simulation.LoadFrame(input.frame);
                }
            }
        }

        public InputFrame Get(int frame)
        {
            int id = frame % size;
            if (frame < 0)
            {
                return new InputFrame
                {
                    frame = 0,
                    moveDir = 0
                };
            }
            else if (buffer[id].frame == -1)
            {
                InputFrame prevInput = Get(frame - 1);
                prevInput.frame = frame;
                buffer[id] = prevInput;
                return prevInput;
            }
            else if (buffer[id].frame != frame)
            {
                InputFrame prevInput = Get(frame - 1);
                prevInput.frame = frame;
                buffer[id] = prevInput;
                return prevInput;
            }
            return buffer[id];
        }
    }

    public InputBufferArray p1inputs;
    public InputBufferArray p2inputs;

    private void Start()
    {
        p1inputs = new InputBufferArray(1000, simulation);
        p2inputs = new InputBufferArray(1000, simulation);
    }
}

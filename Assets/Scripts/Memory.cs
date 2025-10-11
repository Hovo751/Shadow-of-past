using System;
using UnityEngine;

public class Memory : MonoBehaviour
{
    public class PlayerData
    {
        public int playerPos;
    }

    public class MemoryDataArray
    {
        private PlayerData[] data;
        private int size;
        private GameSimulation simulation;
        public MemoryDataArray(int size)
        {
            this.size = size;
            data = new PlayerData[size];
        }

        public void Set(PlayerData dataNow, int frame)
        {
            data[frame % size] = dataNow;
        }

        public PlayerData Get(int frame)
        {
            return data[frame % size];
        }
    }

    public MemoryDataArray player1Data;
    public MemoryDataArray player2Data;

    private void Start()
    {
        player1Data = new MemoryDataArray(1000);
        player2Data = new MemoryDataArray(1000);
    }
}

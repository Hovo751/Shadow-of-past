using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MirrorNetworkManager : NetworkManager
{
    [System.Serializable]
    public class Room
    {
        public long frameDifference;
        public long id;
        public long player1;
        public long player2;
    }
    public MirrorBridge bridge;
    public List<Room> rooms = new();
    public List<PlayerSync> playerList = new();
    private int playersCount = 0;
    private long id = 0;
    PlayerSync[] players = new PlayerSync[2];
    public int GetPlayerCount() { 
        return playersCount; 
    }

    private long GetId()
    {
        if (id == -1) id++;
        return id++;
    }
    [Server]
    private Room FindRoomByPlayer(long player)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].player1 == player || rooms[i].player2 == player)
            {
                return rooms[i];
            }
        }
        return null;
    }[Server]
    private Room FindRoomById(long id)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].id == id)
            {
                return rooms[i];
            }
        }
        return null;
    }
    [Server]
    public long CreateRoom(PlayerSync player)
    {
        if (FindRoomByPlayer(player.id) == null)
        {
            Room newRoom = new Room();
            newRoom.id = GetId();
            newRoom.player1 = player.id;
            newRoom.player2 = -1;
            newRoom.frameDifference = -1;
            rooms.Add(newRoom);
            player.playerNumber = 1;
            player.roomId = newRoom.id;
            player.frameDifference = newRoom.frameDifference;
            return newRoom.id;
        }

        return -1;
    }
    [Server]
    public long JoinRoom(PlayerSync player, long roomId)
    {
        Room currentRoom = FindRoomByPlayer(player.id);
        if (currentRoom == null)
        {
            currentRoom = FindRoomById(roomId);
            if (currentRoom == null)
                return -1;
            if (currentRoom.player2 == -1)
            {
                currentRoom.player2 = player.id;
                currentRoom.frameDifference = bridge.serverFrame;
                if (bridge.serverFrame == -1)
                {
                    currentRoom.frameDifference = 0;
                }
                player.playerNumber = 2;
                player.started = true;
                player.roomId = currentRoom.id;
                player.frameDifference = currentRoom.frameDifference;
                playerList.Find(p => p.id == currentRoom.player1).started = true;
                playerList.Find(p => p.id == currentRoom.player1).frameDifference = currentRoom.frameDifference;
                return currentRoom.id;
            }
        }

        return -1;
    }
    [Server]
    public void DestroyRoom(long id)
    {
        Room currentRoom;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].id == id)
            {
                currentRoom = rooms[i];
                PlayerSync player1 = playerList.Find(p => p.id == currentRoom.player1);
                if (player1 != null)
                {
                    player1.started = false;
                    player1.roomId = -1;
                    player1.playerNumber = 0;
                    player1.frameDifference = -1;
                }
                PlayerSync player2 = playerList.Find(p => p.id == currentRoom.player2);
                if (player2 != null)
                {
                    player2.started = false;
                    player2.roomId = -1;
                    player2.playerNumber = 0;
                    player2.frameDifference = -1;
                }
                rooms.Remove(currentRoom);
                break;
            }
        }
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        playersCount++;

        base.OnServerAddPlayer(conn);
        
        PlayerSync player = conn.identity.GetComponent<PlayerSync>();
        player.id = GetId();
        playerList.Add(player);
        if (playersCount == 1)
        {
            CreateRoom(player);
        }
        if (playersCount == 2)
        {
            JoinRoom(player, rooms[0].id);
        }
        
        return;
        if (playersCount < 3)
        {
            players[playersCount - 1] = conn.identity.GetComponent<PlayerSync>();
            if (playersCount == 2)
            {
                players[0].id = 1;
                players[1].id = 2;
                for (int i = 0; i < 2; i++)
                {
                    players[i].started = true;
                }
                bridge.started = true;
            }
        }
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        PlayerSync player = conn.identity.GetComponent<PlayerSync>();
        playerList.Remove(player);
        Room currentRoom = FindRoomByPlayer(player.id);
        if (currentRoom != null)
            DestroyRoom(currentRoom.id);
        base.OnServerDisconnect(conn);
        Debug.Log("AAAAAAAAAAAAA");
        playersCount--;
        return;
        //bridge.ResetFrames();
        if (playersCount == 1)
        {
            if (players[0] == null)
            {
                players[0] = players[1];
                players[0].id = 1;
                players[1] = null;
            }
            players[0].started = false;
            bridge.started = false;
        }
    }
    /*public override void OnStopServer()
    {
        bridge.ResetFrames();
        bridge.started = false;
        base.OnStopServer();
    }
    public override void OnStopClient()
    {
        bridge.ResetFrames();
        bridge.started = false;
        base.OnStopServer();
    }
    public override void OnStopHost()
    {
        bridge.ResetFrames();
        bridge.started = false;
        base.OnStopServer();
    }*/

}

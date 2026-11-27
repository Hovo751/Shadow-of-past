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
    private ulong roomActionDate = 1;
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
    }
    [Server]
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
    public long CreateRoom(long playerId)
    {
        PlayerSync player = playerList.Find(p => p.id == playerId);
        if (player == null) return -1;
        if (FindRoomByPlayer(player.id) == null)
        {
            Room newRoom = new Room();
            newRoom.id = GetId();
            newRoom.player1 = playerId;
            newRoom.player2 = -1;
            newRoom.frameDifference = -1;
            rooms.Add(newRoom);
            player.playerNumber = 1;
            player.roomId = newRoom.id;
            bridge.RefreshRooms(rooms.ToArray(), roomActionDate++);
            return newRoom.id;
        }

        return -1;
    }
    [Server]
    public long JoinRoom(long playerId, long roomId)
    {
        PlayerSync player = playerList.Find(p => p.id == playerId);
        if (player == null) return -1;
        Room currentRoom = FindRoomByPlayer(playerId);
        if (currentRoom == null)
        {
            currentRoom = FindRoomById(roomId);
            if (currentRoom == null)
                return -1;
            if (currentRoom.player2 == -1)
            {
                currentRoom.player2 = playerId;
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
                playerList.Find(p => p.id == currentRoom.player1).playerNumber = 1;
                playerList.Find(p => p.id == currentRoom.player1).roomId = currentRoom.id;
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
                bridge.RefreshRooms(rooms.ToArray(), roomActionDate++);
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

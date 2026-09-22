using Coherence.Cloud;
using Coherence.Toolkit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Simulation;

public class SimulationNetworking : CoherenceInputSimulation<SimulationState>
{
    // IF SMTH EXPLODES THAN GO TO LINE 1361 IN COHERENCEBRIDGE
    public ComboCounter ComboCounter;
    public int simulationSpeed = 1;
    Dictionary<long, SimulationState> history = new Dictionary<long, SimulationState>();
    private long startFrame = -1;
    private int skipFrames;
    private bool player1LandedHit;
    private bool player2LandedHit;
    public bool drawHitbox = true;
    public TextMeshProUGUI txtDebug;
    public Simulation simulation;

    public Characters characters;

    public Camera _camera;
    protected override void SetInputs(CoherenceClientConnection client)
    {
        var player = client.GameObject.GetComponent<Player>();
        player.SetInput();
    }

    protected override void Simulate(long simulationFrame)
    {
        Player player1 = AllClients[0].GameObject.GetComponent<Player>();
        Player player2 = AllClients[1].GameObject.GetComponent<Player>();

        if (player1.startFrame != player2.startFrame)
        {
            if (player1.startFrame > player2.startFrame)
            {
                player2.startFrame = player1.startFrame;
            }
            else
            {
                player1.startFrame = player2.startFrame;
            }
        }
        startFrame = player1.startFrame;

        int player1MovementInput = (int)player1.GetInput(simulationFrame).movement;
        int player2MovementInput = (int)player2.GetInput(simulationFrame).movement;
        SimulationState currentState = new SimulationState
        {
            PlayerData = new PlayerChangebleStats[AllClients.Count],
            skipFrames = skipFrames,
            player1LandedHit = player1LandedHit,
            player2LandedHit = player2LandedHit,
        };
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            currentState.PlayerData[i] = player.changebleStats;
        }
        try
        {
            history.Add(simulationFrame, currentState);
        }
        catch (ArgumentException)
        {
            history[simulationFrame] = currentState;
        }

        if (player1MovementInput > 9 || player1MovementInput < 1 || player2MovementInput > 9 || player2MovementInput < 1 || startFrame == -1 || startFrame >= simulationFrame || simulationFrame % simulationSpeed != 0)
        {
            return;
        }
        if (player1.changebleStats.Health <= 0 || player2.changebleStats.Health <= 0)
        {
            return;
        }

        MovementAndButtonInput[] player1Inputs = new MovementAndButtonInput[64];
        MovementAndButtonInput[] player2Inputs = new MovementAndButtonInput[64];

        for (long i = simulationFrame; i > simulationFrame - 64; i--)
        {
            MovementAndButtonInput player1Input = new MovementAndButtonInput();
            MovementAndButtonInput player2Input = new MovementAndButtonInput();
            if (i <= startFrame)
            {
                player1Input.movement = 5;
                player1Input.light = false;
                player1Input.medium = false;
                player1Input.heavy = false;
                player2Input.movement = 5;
                player2Input.light = false;
                player2Input.medium = false;
                player2Input.heavy = false;
            }
            else
            {
                player1Input = player1.GetInput(i);
                player2Input = player2.GetInput(i);
            }
            player1Inputs[simulationFrame - i] = player1Input;
            player2Inputs[simulationFrame - i] = player2Input;
        }

        simulation.history = history;
        simulation.startFrame = startFrame;
        currentState = simulation.Simulate(currentState, player1Inputs, player2Inputs, characters.characters[player1.character], characters.characters[player2.character], simulationFrame);

        //Applying the changes

        player1.changebleStats = currentState.PlayerData[0];
        player2.changebleStats = currentState.PlayerData[1];
        skipFrames = currentState.skipFrames;
        player1LandedHit = currentState.player1LandedHit;
        player2LandedHit = currentState.player2LandedHit;

        //saving to history
        history[simulationFrame] = currentState;
    }

    protected override void Rollback(long toFrame, SimulationState state)
    {
        try
        {
            history.Add(toFrame, state);
        }
        catch (ArgumentException)
        {
            history[toFrame] = state;
        }
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            player.changebleStats = state.PlayerData[i];
        }
        skipFrames = state.skipFrames;
        player1LandedHit = state.player1LandedHit;
        player2LandedHit = state.player2LandedHit;
    }

    protected override SimulationState CreateState()
    {
        var simulationState = new SimulationState { 
            PlayerData = new PlayerChangebleStats[AllClients.Count] ,
            skipFrames = skipFrames,
            player1LandedHit = player1LandedHit,
            player2LandedHit = player2LandedHit,
        };
        for (var i = 0; i < AllClients.Count; i++)
        {
            Player player = AllClients[i].GameObject.GetComponent<Player>();
            simulationState.PlayerData[i] = player.changebleStats;
        }

        return simulationState;
    }

    protected override void OnClientJoined(CoherenceClientConnection client)
    {
        if (AllClients.Count >= 2)
        {
            Player player1 = AllClients[0].GameObject.GetComponent<Player>();
            player1.changebleStats.PlayerPositionHorizontal = -5000;
            player1.changebleStats.PlayerPositionVertical = 0;
            player1.changebleStats.IsLookingRight = true;
            Player player2 = AllClients[1].GameObject.GetComponent<Player>();
            player2.changebleStats.PlayerPositionHorizontal = 5000;
            player2.changebleStats.PlayerPositionVertical = 0;
            player1.changebleStats.PlayerAnimationFrame = 0;
            player2.changebleStats.PlayerAnimationFrame = 0;
            player1.changebleStats.juggleScaling = 0;
            player2.changebleStats.juggleScaling = 0;
            player1.changebleStats.Health = 10000;
            player2.changebleStats.Health = 10000;
            ComboCounter.player1 = player1;
            ComboCounter.player2 = player2;
            _camera.player1 = player1.transform;
            _camera.player2 = player2.transform;
            StateStore.Clear();
            SimulationEnabled = AllClients.Count >= 2;
            if (player1.startFrame == -1 && player2.startFrame == -1)
            {
                startFrame = CurrentSimulationFrame + 240;
                player1.startFrame = startFrame;
                player2.startFrame = startFrame;
            }
            else
            {
                startFrame = player1.startFrame;
                if (startFrame == -1)
                {
                    startFrame = player2.startFrame;
                }
            }
        }
    }

    protected override void OnClientLeft(CoherenceClientConnection client)
    {
        SimulationEnabled = AllClients.Count >= 2;
    }

    private void OnDrawGizmos()
    {
        if (AllClients.Count < 2)
            return;
        if (drawHitbox)
        {
            int k = 0;
            foreach (CoherenceClientConnection client in AllClients)
            {
                Player player = client.GameObject.GetComponent<Player>();
                Player otherPlayer = null;
                if (k == 1)
                {
                    otherPlayer = AllClients[0].GameObject.GetComponent<Player>();
                }
                else if (k == 0)
                {
                    otherPlayer = AllClients[1].GameObject.GetComponent<Player>();
                }
                k++;
                AnimBase anim = characters.characters[player.character].animations[player.changebleStats.PlayerAnimation].data;
                float dir = 1;
                int input = player.GetInput().movement;
                if (!player.changebleStats.IsLookingRight)
                {
                    if (input % 3 == 1)
                    {
                        input += 2;
                    }
                    else if (input % 3 == 0)
                    {
                        input -= 2;
                    }
                    dir = -1;
                }
                Rectengale rect;
                Frame frame = anim.frames[player.changebleStats.PlayerAnimationFrame];
                //hurtbox
                Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
                for (int i = 0; i < frame.hurtbox.Length; i++)
                {
                    rect = frame.hurtbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //hitbox
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                for (int i = 0; i < frame.hitbox.Length; i++)
                {
                    rect = frame.hitbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //collisionbox
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                rect = frame.collisionBox;
                Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));

                //throwbox
                Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
                for (int i = 0; i < frame.throwbox.Length; i++)
                {
                    rect = frame.throwbox[i];
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                }
                //blockbox
                Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
                rect = characters.characters[player.character].blockHightBox;
                if (/*input == 4 &&*/ (player.changebleStats.PlayerAnimation == Animations.WalkBack || player.changebleStats.PlayerAnimation == Animations.BlockHigh || player.changebleStats.PlayerAnimation == Animations.BlockLow))
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
                rect = characters.characters[player.character].blockLowBox;
                if (/*input == 1 && */(player.changebleStats.PlayerAnimation == Animations.CrouchBlock || player.changebleStats.PlayerAnimation == Animations.BlockHigh || player.changebleStats.PlayerAnimation == Animations.BlockLow))
                    Gizmos.DrawCube(player.transform.position + new Vector3(rect.posX / 1000.0f * dir, rect.posY / 1000.0f), new Vector3(rect.sizeX / 1000.0f, rect.sizeY / 1000.0f));
            }
        }
    }
}
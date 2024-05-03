using Godot;
using System.Collections.Generic;
using System.Data.SqlTypes;

public partial class GameManager : Node
{
    [Signal]
    public delegate void PlayerRemovedIdEventHandler(int id);

    [Signal]
    public delegate void PlayerAddedIdEventHandler(int id);

    [Signal]
    public delegate void PlayerRemovedEventHandler();

    [Signal]
    public delegate void PlayerAddedEventHandler();

    [Signal]
    public delegate void ClientReadyEventHandler();


    public static List<PlayerInfo> Players = new List<PlayerInfo>();

    private static GameManager _instance = null;

    public bool isReady = false;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameManager();
            }
            return _instance;
        }
    }

    public void ReadyClient()
    {
        isReady = true;
        EmitSignal(SignalName.ClientReady);
    }

    public void AddPlayer(PlayerInfo player)
    {
        if (!Players.Contains(player))
        {
            Players.Add(player);
            EmitSignal(SignalName.PlayerAddedId);
            EmitSignal(SignalName.PlayerAdded);
        }
    }

    public void RemovePlayer(PlayerInfo player)
    {
        if (Players.Contains(player))
        {
            Players.Remove(player);
            EmitSignal(SignalName.PlayerRemovedId);
            EmitSignal(SignalName.PlayerRemoved);
        }
    }

    public PlayerInfo GetPlayerInfo(int id)
    {
        foreach (var player in Players)
        {
            if (player.Id == id)
            {
                return player;
            }
        }
        GD.PrintErr("Invalid ID Provided!");
        return null;
    }
}

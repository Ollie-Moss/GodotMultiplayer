using Godot;
using System;
using System.ComponentModel;
using System.Diagnostics;

public partial class MainSceneManager : Node
{
    private Node players;

    public override void _Ready()
    {
        players = GetNode("Players");
        GameManager.Instance.PlayerRemovedId += (int id) => RemovePlayer(id);
        GameManager.Instance.PlayerAddedId += (int id) => AddPlayer(id);

        if (!Multiplayer.IsServer()) return;
        
        foreach(var player in GameManager.Players)
        {
            if (player.isPlaying)
            {
                GD.Print(player.Id);
                AddPlayer(player.Id);
            }
        }
    }

    public void RemovePlayer(int id)
    {
        if (players.HasNode(id.ToString())) return;
        GetNode(id.ToString()).QueueFree();
    }

    public void AddPlayer(int id)
    {
        Player player = ResourceLoader.Load<PackedScene>("res://Scenes/main_character.tscn").Instantiate<Player>();
        player.Name = id.ToString();
        player.PlayerId = id;

        SpawnPointComponent spawnPoint = GetNode<SpawnerComponent>("SpawnerComponent").FindNextSpawnPoint();
        player.GlobalPosition = spawnPoint.GlobalPosition;

        players.AddChild(player);
        player.SetUpPlayer(GameManager.Instance.GetPlayerInfo(id));

    }
}

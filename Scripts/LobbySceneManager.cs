using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class LobbySceneManager : Node2D
{
    private ItemList _playerItemList;

    public override void _Ready()
    {
        _playerItemList = GetNode<ItemList>("PlayersItemList");
        GameManager.Instance.PlayerAdded += () => UpdatePlayerList();
        GameManager.Instance.PlayerRemoved += () => UpdatePlayerList();
        GetNode<Button>("Start Game").Pressed += () => StartGame();
        if (!Multiplayer.IsServer())
        {
            GetNode<Button>("Start Game").Disabled = true;
        }
        UpdatePlayerList();
    }

    public void UpdatePlayerList()
    {
        _playerItemList.Clear();
        foreach(var player in GameManager.Players)
        {
            _playerItemList.AddItem($"Name: {player.Name}", null, false);
        }
    }

    public void StartGame()
    {
        foreach(var player in GameManager.Players)
        {
            player.isPlaying = true;
        }
        GetNode<MultiplayerController>("/root/MultiplayerController").LoadScene(ResourceLoader.Load<PackedScene>("res://Scenes/main.tscn"));
    }
}

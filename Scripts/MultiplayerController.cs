using Godot;
using System;
using System.Linq;

public partial class MultiplayerController : Control
{
	[Export]
	private int port = 25565;

	[Export]
	private string ipAddress = "127.0.0.1";

	private ENetMultiplayerPeer peer;

	public override void _Ready()
	{
		GetNode<ClientManager>("Hole Punching Component").HolePunched += joinGame;
		GetNode<ClientManager>("Hole Punching Component").HostStarted += hostGame;
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
		if (OS.GetCmdlineArgs().Contains("--server"))
		{
			hostGame();
		}
	}

	/// <summary>
	/// Runs only on clients when connection failed
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	private void ConnectionFailed()
	{
		GD.Print("Connection Failed!");
	}

	/// <summary>
	/// Runs only on client when connection is successful
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	private void ConnectedToServer()
	{
		GD.Print("Connected to server");
		RpcId(1, nameof(sendPlayerInfo), GetNode<LineEdit>("UI/NameInput").Text, Multiplayer.GetUniqueId());
	}

	/// <summary>
	/// Run on all peers when a player disconnects
	/// </summary>
	/// <param name="id">id of the player that disconnected</param>
	/// <exception cref="NotImplementedException"></exception>
	private void PeerDisconnected(long id)
	{
		GD.Print("Player Disconnected: " + id.ToString());
		GameManager.Players.Remove(GameManager.Players.Where(i => i.Id == id).First<PlayerInfo>());
		var players = GetTree().GetNodesInGroup("Player");
		foreach(var player in players)
		{
			if(player.Name == id.ToString())
			{
				//player.QueueFree();
			}
		}
	}

	/// <summary>
	/// Runs on all peers when a player connects
	/// </summary>
	/// <param name="id">id of the player that connected</param>
	/// <exception cref="NotImplementedException"></exception>
	private void PeerConnected(long id)
	{
		GD.Print("Player Connected: " + id.ToString());
	}

	private void hostGame()
	{
		

		GD.Print("Creating Server...");
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 10);
		if (error != Error.Ok)
		{
			GD.Print("Error failed to create server: " + error);
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Server Successfully Created!\nWaiting For Players!");

		StartGame();
		sendPlayerInfo(GetNode<LineEdit>("UI/NameInput").Text, 1);
	}

	public void _on_host_button_down()
	{
		GetNode<ClientManager>("Hole Punching Component").StartTraversel(true); 
		
	}

	public void _on_join_button_down()
	{
        GetNode<ClientManager>("Hole Punching Component").StartTraversel(false);
    }

	public void joinGame()
	{
		GD.Print("Connecting to Server...");
		ipAddress = GetNode<LineEdit>("IP Input").Text;
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateClient(ipAddress, port, localPort: 25565);
		if (error != Error.Ok)
		{
			GD.Print("Error failed to connect to server: " + error);
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Successfully Connected to Server!");
		// Load correct scene
		StartGame();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void sendPlayerInfo(string name, int id)
	{
		PlayerInfo playerInfo = new PlayerInfo()
		{
			Name = name,
			Id = id
		};
		if (GameManager.Players.Where(i => i.Id == id).Count() == 0) 
		{
			GameManager.Instance.AddPlayer(playerInfo);
		}
		if (Multiplayer.IsServer())
		{
			GameManager.Instance.ReadyClient();
			foreach(var player in GameManager.Players)
			{
				Rpc(nameof(sendPlayerInfo), player.Name, player.Id);
			}
			//RpcId(id, nameof(readyClient));
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	private void readyClient()
	{
		GameManager.Instance.ReadyClient();
	}


	private void StartGame()
	{
		GetNode<Control>("UI").Hide();
		GetTree().Paused = false;

		if (Multiplayer.IsServer())
		{
			CallDeferred(nameof(LoadScene), ResourceLoader.Load<PackedScene>("res://Scenes/Lobby.tscn"));
		}
	}

	public void LoadScene(PackedScene scene)
	{
		Node prevScene = GetNode<Node>("Multiplayer/Scene");

		foreach(var child in prevScene.GetChildren())
		{
			prevScene.RemoveChild(child);
			child.QueueFree();
		}

		prevScene.AddChild(scene.Instantiate());
	}
}

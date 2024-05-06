using Godot;
using System;

public partial class NetworkManager : Control
{
	ClientManager holePunchingNode;

    [Export]
    private int port = 25565;

    [Export]
    private string ipAddress = "127.0.0.1";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		holePunchingNode = GetNode<ClientManager>("Hole Punching Component");

		holePunchingNode.HostStarted += hostStarted;
		holePunchingNode.HolePunched += joinedGame;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_join_button_pressed()
	{
		holePunchingNode.StartTraversel(port, false, GetNode<LineEdit>("CodeInput").Text);
	}

	public void _on_host_button_pressed()
	{
        holePunchingNode.StartTraversel(port, false, "AAAA");
    }

	private void hostStarted(string code)
	{
		GetNode<Control>("UI/JoinUI").Hide();
		GetNode<Label>("UI/OR Label").Hide();

        GetNode<Label>("UI/HostUI/GameCode Label").Text = $"Game Code: {code}";
        GetNode<Label>("UI/HostUI/GameCode Label").Show();
	}

    private void joinedGame()
    {

    }
}

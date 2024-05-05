
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Data;
using System;
using Godot;
using System.Runtime.CompilerServices;

public partial class HolePunchingClient : Node2D
{

    [Signal]
	public delegate void HolePunchedEventHandler();
	
	[Signal]
	public delegate void HostStartedEventHandler();
	
	private bool _clientRunning = false;
	private bool _isHost;

	public UdpClient client;

	private const string _hostRequest = "hr";
	private const string _peerPunching = "pp";
	private const string _holePunch = "hp";

	[Export]
	private string ServerIP = "127.0.0.1";
	[Export]
	private int ServerPort = 25565;

	// endpoint where server is listening
	private IPEndPoint serverEP;

	public void StartClient(bool isHost)
	{
		_isHost = isHost;
		serverEP = new IPEndPoint(IPAddress.Parse(ServerIP), ServerPort);
		client = new UdpClient();

		//client.Connect(serverEP);

		_clientRunning = true;


		if (isHost)
		{
			SendBytes("hr:PLEASE", serverEP);
		}
		else
		{
			SendBytes("cr:AAAA", serverEP);
		}

		while (_clientRunning)
		{
			var data = client.Receive(ref serverEP);

			GD.Print($"receive data from {serverEP.ToString()}");

			string[] packet = Encoding.ASCII.GetString(data).Split(":");
			HandlePacket(packet);
		}

	}

	// Handle incoming udp packet
	public void HandlePacket(string[] packet)
	{
		switch (packet[0])
		{
			case _holePunch:
				HolePunch(packet);
				return;
			case _hostRequest:
				HostRequest(packet);
				return;
			case _peerPunching:
				PeerPunching(packet);
				return;
			default:
				Console.WriteLine("Invalid packet recieved from: {0}", serverEP.ToString());
				return;
		}


		/*Console.WriteLine("Message received from {0}:", Remote.ToString());
		Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));*/
	}

	// Is server alive
	public void PeerPunching(string[] packet)
	{
		if(!_isHost)
		{
			EmitSignal(SignalName.HolePunched);
		}
		
		CloseClient();
	}

	public void HostRequest(string[] packet)
	{
		if (packet[1] == "OK")
		{
			GD.Print("Lobby Registered Created");
			EmitSignal(SignalName.HostStarted);
		}
	}

	public void HolePunch(string[] packet)
	{
		SendBytes("pp:Punching", new IPEndPoint(IPAddress.Parse(packet[1]), int.Parse(packet[2])));
	}

	public void SendBytes(string data, IPEndPoint endpoint)
	{
		byte[] byteArray = Encoding.UTF8.GetBytes(data);
		Console.WriteLine($"Sending data to: {endpoint.ToString()}");
		client.Send(byteArray, endpoint);
	}

	public void CloseClient()
	{
		client.Close();
		_clientRunning = false;
	}
}



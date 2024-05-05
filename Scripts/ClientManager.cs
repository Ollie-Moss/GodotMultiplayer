using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


public partial class ClientManager : Node2D
{
    [Signal]
    public delegate void HolePunchedEventHandler();

    [Signal]
    public delegate void HostStartedEventHandler();


    private PacketPeerUdp _server = new PacketPeerUdp();
    private PacketPeerUdp _peer = new PacketPeerUdp();


    [Export]
    private string serverIP = "127.0.0.1";
    [Export]
    private int serverPort = 25565;

    private bool isHost = false;

    private const string _hostRequest = "hr";
    private const string _peerPunching = "pp";
    private const string _holePunch = "hp";


    public override void _Process(double delta)
    {
       if (_server.GetAvailablePacketCount() > 0)
       {
            string rawPacket = _server.GetPacket().GetStringFromUtf8();
            GD.Print($"Recieved Packet From: {_server.GetPacketIP}:{_server.GetPacketPort}, Data: {rawPacket}");

            HandlePacket(rawPacket.Split(':'));
       }
    }

    public void StartTraversel(bool _isHost)
    {
        isHost = _isHost;

        _server.Bind(11101);
        _server.SetDestAddress(serverIP, serverPort);

        if (isHost)
        {
            _server.PutPacket("hr:PLEASE".ToUtf8Buffer());
        }
        else
        {
            _server.PutPacket("cr:AAAA".ToUtf8Buffer());
        }
    }

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
                Console.WriteLine("Invalid packet recieved");
                return;
        }
    }

    public void PeerPunching(string[] packet)
    {
        if (!isHost)
        {
            EmitSignal(SignalName.HolePunched);
        }

        _peer.Close();
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
        _peer.SetDestAddress(packet[1], int.Parse(packet[2]));
        _peer.Bind(25565);

        _peer.PutPacket("pp:Punching".ToUtf8Buffer());
    }
}


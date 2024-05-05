public class Lobby
{
    private string _gameCode;
    private string _ipAddress;
    private int _portNumber;

    public string GameCode { get => _gameCode; set => _gameCode = value; }
    public int PortNumber { get => _portNumber; set => _portNumber = value; }
    public string IpAddress { get => _ipAddress; set => _ipAddress = value; }
}

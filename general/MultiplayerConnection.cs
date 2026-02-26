using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using BoardGames.general;

public partial class MultiplayerConnection : Node {
    public const long ServerId = MultiplayerPeer.TargetPeerServer;
    public const long BroadcastId = MultiplayerPeer.TargetPeerBroadcast;
    public const int CommChannel = 4;
    
    private int _maxPlayerCount = 2;
    private Dictionary<long, string> _idToPlayer = new ();

    private string _playerName;
    private string _ip = "localhost";
    private int _port = -1;
    
    [Export]
    public int MaxPlayerCount {
        private set {
            if (value >= 2) {
                _maxPlayerCount = value;
            }
        }
        get => _maxPlayerCount;
    }
    
    [Export]
    public int CurrentPlayerCount { get; private set; } = 1;

    public override void _Ready() {
        if (OS.GetCmdlineArgs().Contains("Anna")) {
            SetupServer("Anna", 2, 2000);
        } else {
            var name = OS.GetCmdlineArgs()[^1];
            SetupClient(name, "localhost", 2000);
        }
    }

    public bool SetupClient(string playerName, string ip, int port) {
        _playerName = playerName;
        _ip = ip;
        _port = port;
        
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(ip, port);
        if (error != 0) {
            return false;
        }
        
        (Multiplayer as SceneMultiplayer).PeerPacket += OnPacketReceived;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        
        Multiplayer.MultiplayerPeer = peer;
        UpdateConnectionInfo();

        return true;
    }

    private void ConnectedToServer() {
        SendData(BroadcastId, TransmissionCodes.PlayerNameTransmission, _playerName);
        Multiplayer.ConnectedToServer -= ConnectedToServer;
    }
    
    public bool SetupServer(string playerName, int maxPlayerCount, int port) {
        _playerName = playerName;
        MaxPlayerCount = maxPlayerCount;
        _port = port;
        
        var ipQuery = GetNode<HttpRequest>("IPQuery");
        ipQuery.Request("https://api.ipify.org", []);
        
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port, MaxPlayerCount);
        if (error != 0) {
            return false;
        }
        
        (Multiplayer as SceneMultiplayer).PeerPacket += OnPacketReceived;
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        
        Multiplayer.MultiplayerPeer = peer;

        _idToPlayer[ServerId] = playerName;
        return true;
    }

    public void SendData(long target, TransmissionCodes transmissionId, params object[] data) {
        int totalSize = sizeof(int);
        if (data.Length > 0) {
            foreach (object obj in data) {
                totalSize += obj switch {
                    int i => sizeof(int),
                    long i => sizeof(long),
                    float f => sizeof(float),
                    double d => sizeof(double),
                    bool b => sizeof(bool),
                    string s => s.Length,
                    _ => throw new NotImplementedException()
                };
            }
        }
        byte[] packet = new byte[totalSize];
        Buffer.BlockCopy(BitConverter.GetBytes((int) transmissionId), 0, packet, 0, sizeof(int));
        int offset = sizeof(int);
        foreach (object obj in data) {
            switch (obj) {
                case int i:
                    Buffer.BlockCopy(BitConverter.GetBytes(i), 0, packet, offset, sizeof(int));
                    offset += sizeof(int);
                    break;
                case long l:
                    Buffer.BlockCopy(BitConverter.GetBytes(l), 0, packet, offset, sizeof(long));
                    offset += sizeof(long);
                    break;
                case float f:
                    Buffer.BlockCopy(BitConverter.GetBytes(f), 0, packet, offset, sizeof(float));
                    offset += sizeof(float);
                    break;
                case double d:
                    Buffer.BlockCopy(BitConverter.GetBytes(d), 0, packet, offset, sizeof(double));
                    offset += sizeof(double);
                    break;
                case bool b:
                    Buffer.BlockCopy(BitConverter.GetBytes(b), 0, packet, offset, sizeof(bool));
                    offset += sizeof(bool);
                    break;
                case string s:
                    byte[] stringBytes = Encoding.UTF8.GetBytes(s);
                    Buffer.BlockCopy(stringBytes, 0, packet, offset, stringBytes.Length);
                    offset += stringBytes.Length;
                    break;
            }
        }
        
        (Multiplayer as SceneMultiplayer).SendBytes(packet, (int) target, MultiplayerPeer.TransferModeEnum.Reliable, CommChannel);
    }

    private void UpdateConnectionInfo() {
        var label = GetNode<Label>("ConnectionInfo");
        label.Text = $"IP:{_ip}\nPort: {_port}";
        label.Show();
    }
    
    private void OnPeerConnected(long id) {
        CurrentPlayerCount++;
    }

    private void OnPeerDisconnected(long id) {
        if (Multiplayer.IsServer()) {
            CurrentPlayerCount--;
        }
        _idToPlayer.Remove(id);
    }
    
    private void OnPacketReceived(long id, byte[] data) {
        var transmissionId = (TransmissionCodes) BitConverter.ToInt32(data, 0);
        switch (transmissionId) {
            case TransmissionCodes.PlayerNameTransmission: {
                string playerName = Encoding.UTF8.GetString(data, sizeof(int), data.Length - sizeof(int));
                _idToPlayer[id] = playerName;
                SendData(id, TransmissionCodes.PlayerNameTransmissionResponse, _playerName);
                break;
            }
            case TransmissionCodes.PlayerNameTransmissionResponse: {
                string playerName = Encoding.UTF8.GetString(data, sizeof(int), data.Length - sizeof(int));
                _idToPlayer[id] = playerName;
                break;
            }
        }
    }
    
    // requestCompleted from HTTPRequest (IPQuery)
    private void OnRequestCompleted(int result, int responseCode, string[] headers, byte[] body) {
        if (responseCode == 200) {
            _ip = body.GetStringFromUtf8();
            UpdateConnectionInfo();
        } else {
            GD.PushError("Request failed");
        }
    }
    
    // GuiInput from Label (ConnectionInfo)
    private void OnLabelGuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
            DisplayServer.ClipboardSet($"{_ip}:{_port}");
        }
    }

}

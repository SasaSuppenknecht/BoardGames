using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BoardGames.general;

public partial class MultiplayerConnection : Node {
    
    public const long ServerId = MultiplayerPeer.TargetPeerServer;
    public const long BroadcastId = MultiplayerPeer.TargetPeerBroadcast;

    [Export] public int MaxPlayerCount {
        private set {
            if (value >= 2) {
                _maxPlayerCount = value;
            }
        }
        get => _maxPlayerCount;
    }
    private int _maxPlayerCount = 2;

    [Export] public int CurrentPlayerCount { get; private set; } = 1;

    [Export] private bool Debug = false;
    
    public string PlayerName { get; private set; }

    private Godot.Collections.Dictionary<long, string> _idToPlayer;
    private string _ip = "localhost";
    private int _port = -1;
    
    public override void _Ready() {
        if (Debug) {
            if (OS.GetCmdlineArgs().Contains("Anna")) {
                var name = OS.GetCmdlineArgs()[^1];
                SetupServer(name, MaxPlayerCount, 30000);
                GetParent().CallDeferred(
                    Node.MethodName.AddChild, 
                    ResourceLoader.Load<PackedScene>("res://Schafkopf/Schafkopf.tscn").Instantiate()
                    );
            } else {
                var name = OS.GetCmdlineArgs()[^1];
                SetupClient(name, "localhost", 30000);
            }
        }
    }

    public bool SetupClient(string playerName, string ip, int port) {
        PlayerName = playerName;
        _ip = ip;
        _port = port;
        
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(ip, port);
        if (error != 0) {
            return false;
        }
        
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        
        Multiplayer.MultiplayerPeer = peer;
        UpdateConnectionInfo();

        return true;
    }

    public bool SetupServer(string playerName, int maxPlayerCount, int port) {
        PlayerName = playerName;
        MaxPlayerCount = maxPlayerCount;
        _port = port;
        
        var ipQuery = GetNode<HttpRequest>("IPQuery");
        ipQuery.Request("https://api.ipify.org", []);
        
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port, MaxPlayerCount);
        if (error != 0) {
            return false;
        }
        
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        
        Multiplayer.MultiplayerPeer = peer;

        _idToPlayer = new();
        _idToPlayer[ServerId] = playerName;
        return true;
    }

    public string GetNameOfId(long id) {
        bool isPresent = _idToPlayer.TryGetValue(id, out string name);
        if (isPresent) {
            return name;
        } else {
            return null;
        }
    }

    public void TransmitData() {
        Rpc(MethodName.TransmitPlayerNamesToClient, _idToPlayer);
    }
    
    private void UpdateConnectionInfo() {
        var label = GetNode<Label>("ConnectionInfo");
        label.Text = $"IP: {_ip}\nPort: {_port}";
        label.Show();
    }
    
    private void OnConnectedToServer() {
        RpcId(ServerId, MethodName.TransmitPlayerNameToServer, PlayerName);
    }
    
    private void OnPeerConnected(long id) {
        CurrentPlayerCount++;
    }

    private void OnPeerDisconnected(long id) {
        CurrentPlayerCount--;
        if (Multiplayer.IsServer()) {
            _idToPlayer.Remove(id);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void TransmitPlayerNameToServer(string playerName) {
        var senderId = Multiplayer.GetRemoteSenderId();
        _idToPlayer[senderId] = playerName;
        GD.Print($"{_idToPlayer} on {PlayerName}");
    }

    [Rpc]
    private void TransmitPlayerNamesToClient(Godot.Collections.Dictionary<long, string> idToPlayer) {
        _idToPlayer = idToPlayer;
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
            DisplayServer.ClipboardSet($"{_ip}");
        }
    }

}

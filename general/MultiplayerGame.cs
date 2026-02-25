using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

[GlobalClass]
public abstract partial class MultiplayerGame : Node {
    public const long ServerId = MultiplayerPeer.TargetPeerServer;

    [Signal] public delegate void GameStartedEventHandler();
    
    private int _maxPlayers = 2;
    private Dictionary<long, string> _idToPlayer = new ();

    [Export] private bool Debug = false;
    
    [Export]
    public int MaxPlayers {
        private set {
            if (value >= 2) {
                _maxPlayers = value;
            }
        }
        get => _maxPlayers;
    }

    public int CurrentPlayers { get; protected set; } = 1;

    public override void _EnterTree() {
        (Multiplayer as SceneMultiplayer).PeerPacket += OnPacketReceived;
        
        if (Debug) {
            var cmd = OS.GetCmdlineArgs();
            ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
            if (cmd.Contains("--server")) {
                peer.CreateServer(2000, MaxPlayers);

                Multiplayer.PeerConnected += OnPeerConnected;
                Multiplayer.PeerDisconnected += OnPeerDisconnected;
            } else {
                peer.CreateClient("localhost", 2000);
            }
            Multiplayer.MultiplayerPeer = peer;
            return;
        }
        
        if (GlobalData.Instance.IsHost) {
            _idToPlayer.Add(ServerId, GlobalData.Instance.PlayerName);
            SetupServer();
        }
    }

    public abstract void StartGame();
    
    protected virtual void OnPeerConnected(long id) {
        CurrentPlayers++;
    }

    protected virtual void OnPeerDisconnected(long id) {
        CurrentPlayers--;
        _idToPlayer.Remove(id);
    }

    private void OnPacketReceived(long id, byte[] data) {
        string playerName = Encoding.UTF8.GetString(data);
        _idToPlayer.TryAdd(id, playerName);
        
        if (Multiplayer.IsServer()) {
            (Multiplayer as SceneMultiplayer).SendBytes(
                data, (int)MultiplayerPeer.TargetPeerBroadcast, MultiplayerPeer.TransferModeEnum.Reliable, GlobalData.COMM_CHANNEL
                );
        }
    }
    
    
    private void SetupServer() {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateServer(GlobalData.Instance.Port, MaxPlayers);
        
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        
        Multiplayer.MultiplayerPeer = peer;
    }
    
}
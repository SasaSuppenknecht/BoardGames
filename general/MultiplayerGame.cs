using Godot;

[GlobalClass]
public partial class MultiplayerGame : Node {
    
    private int _maxPlayers = 2;
    
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
    protected readonly int ServerId = 1;

    public MultiplayerGame() {
        if (GlobalData.Instance.IsHost) {
            SetupServer();
        }
    }

    protected virtual void OnPeerConnected(long id) {
        CurrentPlayers++;
    }

    protected virtual void OnPeerDisconnected(long id) {
        CurrentPlayers--;
    }
    
    private void SetupServer() {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        peer.CreateServer(MaxPlayers, GlobalData.Instance.Port);
        
        var multiplayer = GetTree().GetMultiplayer();
        multiplayer.MultiplayerPeer = peer;

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
    }
    
}
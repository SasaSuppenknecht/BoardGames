using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public abstract partial class TurnBasedMultiplayerGame : Node {
    [Signal] public delegate void TurnChangedEventHandler(bool isTurn);
    
    public MultiplayerConnection MultiplayerConnection;

    public bool IsTurn {
        get => _isTurn;
        protected set {
            EmitSignalTurnChanged(value);
            _isTurn = value;
        }
    }
    private bool _isTurn;
    
    public long CurrentPlayer { get; private set; } = -1;

    protected List<long> PeerOrder;
    
    public override void _Ready() {
        var node = GetNode("../MultiplayerConnection");
        if (node is MultiplayerConnection multiplayerConnection) {
            MultiplayerConnection = multiplayerConnection;
        } else {
            GD.PushError("Could not find MultiplayerConnection");
        }
        
        if (Multiplayer.IsServer()) {
            PeerOrder = new() {MultiplayerConnection.ServerId};
            Multiplayer.PeerConnected += OnPeerConnected;
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
        }
    }
    
    public void EndTurn() {
        IsTurn = false;
        RpcId(MultiplayerConnection.ServerId, MethodName.EndTurnMessage);
    }

    public virtual void StartGame() {
        Rpc(MethodName.TransmitPlayerOrder, PeerOrder.ToArray());
        Rpc(MethodName.AnnounceNextPlayer, PeerOrder[1]); // todo change to 0
    } 

    public abstract int GetMaxPlayerCount();

    protected virtual long DetermineNextPlayer() {
        int index = PeerOrder.IndexOf(CurrentPlayer);
        int nextIndex = (index + 1) % PeerOrder.Count;
        return PeerOrder[nextIndex];
    }
    
    protected virtual void OnPeerConnected(long id) {
        PeerOrder.Add(id);
    }

    protected virtual void OnPeerDisconnected(long id) {
        // todo probably should just kill game if one player disconnects
        PeerOrder.Remove(id);
    }
    

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void EndTurnMessage() {
        if (Multiplayer.IsServer()) {
            var peerId = Multiplayer.GetRemoteSenderId();
            if (peerId != CurrentPlayer) { // check if peer is legit
                GD.PushWarning("Received EndTurnMessage from illegal peer with id: " + peerId);
                return;
            }

            long next = DetermineNextPlayer();
            Rpc(MethodName.AnnounceNextPlayer, next);
        }
    }
    
    [Rpc]
    private void TransmitPlayerOrder(long[] peerOrder) {
        PeerOrder = peerOrder.ToList();
        //todo debug only, remove later
        GetNode<Label>("%Label2").Text = (PeerOrder.IndexOf(Multiplayer.GetUniqueId()) + 1).ToString();
    }
    
    [Rpc(CallLocal = true)]
    private void AnnounceNextPlayer(long id) {
        CurrentPlayer = id;
        if (Multiplayer.GetUniqueId() == id) {
            IsTurn = true;  
        }
    }
}

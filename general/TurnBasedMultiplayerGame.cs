using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public abstract partial class TurnBasedMultiplayerGame : Node {
    [Signal] public delegate void TurnBeginsEventHandler();
    
    public MultiplayerConnection MultiplayerConnection;
    
    public long CurrentPlayer { get; private set; } = -1;

    protected List<long> PeerOrder;
    
    public override void _Ready() {
        // todo check if this is needed
        var node = GetNode("../../MultiplayerConnection");
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
        RpcId(MultiplayerConnection.ServerId, MethodName.EndTurnMessage);
    }

    public virtual void StartGame() {
        if (Multiplayer.IsServer()) {
            Rpc(MethodName.TransmitPlayerOrder, PeerOrder.ToArray());
        }
        Rpc(MethodName.AnnounceNextPlayer, PeerOrder[0]);
    }

    protected virtual long DetermineNextPlayer() {
        int index = PeerOrder.IndexOf(CurrentPlayer);
        int nextIndex = (index + 1) % PeerOrder.Count;
        return PeerOrder[nextIndex];
    }
    
    private void OnPeerConnected(long id) {
        PeerOrder.Add(id);
    }

    private void OnPeerDisconnected(long id) {
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
    }
    
    [Rpc(CallLocal = true)]
    private void AnnounceNextPlayer(long id) {
        CurrentPlayer = id;
        if (Multiplayer.GetUniqueId() == id) {
            EmitSignalTurnBegins();
        }
    }
}

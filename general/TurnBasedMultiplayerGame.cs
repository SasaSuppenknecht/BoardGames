using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class TurnBasedMultiplayerGame : MultiplayerGame {
    

    [Signal]
    public delegate void TurnBeginsEventHandler();

    public long CurrentPlayer { get; private set; } = -1;

    protected List<long> PeerOrder;
    
    public override void _Ready() {
        base._Ready();

        if (Multiplayer.IsServer()) {
            PeerOrder = new() {ServerId};
        }
    }
    
    public void EndTurn() {
        RpcId(ServerId, MethodName.EndTurnMessage);
    }

    public override void StartGame() {
        if (Multiplayer.IsServer()) {
            Rpc(MethodName.TransmitPlayerOrder, PeerOrder.ToArray());
            Rpc(MethodName.GameStartedMessage);
        }
        Rpc(MethodName.NextPlayer, PeerOrder[0]);
    }

    protected override void OnPeerConnected(long id) {
        PeerOrder.Add(id);
        base.OnPeerConnected(id);
    }

    protected override void OnPeerDisconnected(long id) {
        // todo probably should just kill game if one player disconnects
        PeerOrder.Remove(id);
        base.OnPeerDisconnected(id);
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
            Rpc(MethodName.NextPlayer, next);
        }
    }

    protected virtual long DetermineNextPlayer() {
        int index = PeerOrder.IndexOf(CurrentPlayer);
        int nextIndex = (index + 1) % PeerOrder.Count;
        return PeerOrder[nextIndex];
    }

    [Rpc(CallLocal = true)]
    private void NextPlayer(long id) {
        CurrentPlayer = id;
        if (Multiplayer.GetUniqueId() == id) {
            EmitSignalTurnBegins();
        }
    }

    [Rpc]
    private void TransmitPlayerOrder(long[] peerOrder) {
        PeerOrder = peerOrder.ToList();
    }

    [Rpc(CallLocal = true)]
    private void GameStartedMessage() {
        EmitSignalGameStarted();
    }
}

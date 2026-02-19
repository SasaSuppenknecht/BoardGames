using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TurnBasedMultiplayerGame : MultiplayerGame {
    

    [Signal]
    public delegate void TurnBeginsEventHandler();

    private List<long> _peerOrder;
    private int _current = 0;
    
    public override void _Ready() {
        base._Ready();

        if (Multiplayer.IsServer()) {
            _peerOrder = new() {ServerId};
        }
    }
    
    public void EndTurn() {
        RpcId(ServerId, MethodName.EndTurnMessage);
    }
    
    protected override void OnPeerConnected(long id) {
        _peerOrder.Add(id);
        base.OnPeerConnected(id);
    }

    protected override void OnPeerDisconnected(long id) {
        // todo this needs to do more checking if _current is outside the range
        _peerOrder.Remove(id);
        base.OnPeerDisconnected(id);
    }
    

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void EndTurnMessage() {
        var peerId = Multiplayer.GetRemoteSenderId();
        int index = _peerOrder.IndexOf(peerId);
        if (_current != index) { // check if peer is legit
            GD.PushWarning("Received EndTurnMessage from illegal peer with id: " + peerId);
            return;
        }
        // determine next player and tell it that its turn has begun
        _current = (_current + 1) % _peerOrder.Count;
        RpcId(_peerOrder[_current], MethodName.BeginTurn);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void BeginTurn() {
        EmitSignalTurnBegins();
    }
    
}

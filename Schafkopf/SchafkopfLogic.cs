using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames.Schafkopf;

public partial class SchafkopfLogic : TurnBasedMultiplayerGame {
    
    [Export] private Container _middle;
    [Export] private Container _deck;
    
    [Export] private PackedScene _cardScene;

    private List<CardType> _playedCards = new (4);
    
    protected override void OnPeerConnected(long id) {
        base.OnPeerConnected(id);

        if (CurrentPlayers == MaxPlayers) {
            StartGame();
        }
    }

    public void PlayCard(int cardType) {
        Rpc(MethodName._PlayCard, cardType);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void _PlayCard(int cardType) {
        if (Multiplayer.IsServer()) {
            Card card = _cardScene.Instantiate<Card>();
            card.Type = (CardType)cardType;
            _middle.AddChild(card, true);
        } 
        _playedCards.Add((CardType) cardType);
    }

    public override void StartGame() {
        Random random = new Random();
        var values = ((CardType[]) Enum.GetValues(typeof(CardType)))
                .Where(type => !CardTypeInfo.NotSchafkopfCards.Contains(type))
                .OrderBy(type => random.Next())
                .ToArray();
        
        int[] shuffledValues = Array.ConvertAll(values, type => (int) type);
        
        for (int i = 0; i < MaxPlayers; i++) {
            int[] slice = shuffledValues[(i * 8)..((i + 1) * 8)];
            long targetId = PeerOrder[i];
            RpcId(targetId, MethodName.TransmitCards, slice);
        }
        
        base.StartGame();
    }

    protected override long DetermineNextPlayer() {
        if (_middle.GetChildCount() == 4) {
            
            
            _playedCards.Clear();
            return 0;
        } else {
            return base.DetermineNextPlayer();
        }
    }

    [Rpc(CallLocal = true)]
    private void TransmitCards(int[] cards) {
        foreach (int cardType in cards) {
            Card card = _cardScene.Instantiate<Card>();
            card.Type = (CardType)cardType;
            _deck.AddChild(card, true);
        }
    }
    
}

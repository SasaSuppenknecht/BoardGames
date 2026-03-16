using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames.Schafkopf;

public partial class Schafkopf : TurnBasedMultiplayerGame {
    
    [Export] private GameBoard _gameBoard;
    
    private List<CardType> _playedCards = new(4);
    private SchafkopfLogic _schafkopfLogic = new();

    public override int GetMaxPlayerCount() {
        return 4;
    }

    public override void StartGame() {
        Random random = new Random();
        var values = ((CardType[]) Enum.GetValues(typeof(CardType)))
                .Where(type => !CardTypeInfo.NotSchafkopfCards.Contains(type))
                .OrderBy(type => random.Next())
                .ToArray();
        
        int[] shuffledValues = Array.ConvertAll(values, type => (int) type);
        
        for (int i = 0; i < MultiplayerConnection.MaxPlayerCount; i++) {
            int[] slice = shuffledValues[(i * 8)..((i + 1) * 8)];
            long targetId = PeerOrder[i];
            RpcId(targetId, MethodName.TransmitCards, slice);
        }
        
        base.StartGame();
    }

    protected override long DetermineNextPlayer() {
        if (_playedCards.Count == 4) {
            // of the played cards, determine which card wins the trick
            int winner = _schafkopfLogic.DetermineTrickWinner(_playedCards);
            // get index of current player...
            int indexOfCurrentPlayer = PeerOrder.IndexOf(CurrentPlayer);
            // ... in order to determine the next player based on the winner index
            long nextPlayer = PeerOrder[(indexOfCurrentPlayer + 1 + winner) % 4];
            
            Rpc(MethodName._AwardTrick, nextPlayer);
            Rpc(MethodName._ClearPlayedCards);
            
            return nextPlayer;
        } else {
            return base.DetermineNextPlayer();
        }
    }

    protected override void OnPeerConnected(long id) {
        base.OnPeerConnected(id);
        if (PeerOrder.Count == MultiplayerConnection.MaxPlayerCount) {
            StartGame();
        }
    }

    private void OnTurnChanged(bool isMyTurn) {
        Card[] cards = _gameBoard.GetCardsInDeck();
        if (isMyTurn) {
            CardType? firstCard = _playedCards.Count > 0 ? _playedCards[0] : null;
            List<int> indices = _schafkopfLogic.DetermineAllowedCardsToPlay(cards.Select(card => card.Type).ToList(), firstCard);
            for (int index = 0; index < cards.Length; index++) {
                if (!indices.Contains(index)) {
                    cards[index].Clickable = false;
                }
            }
        } else { // generally enable cards for next turn again
            foreach (var card in cards) {
                card.Clickable = true;
            }
        }
    }
    
    [Rpc(CallLocal = true)]
    private void TransmitCards(int[] cards) {
        foreach (int cardType in cards) {
            _gameBoard.AddCardToDeck((CardType) cardType);
        }
    }

    private void PlayCard(Card card) {
        RpcId(MultiplayerConnection.ServerId, MethodName._PlayCard, (int)card.Type);
        EndTurn();
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void _PlayCard(int cardType) {
        _playedCards.Add((CardType) cardType);
        Rpc(MethodName._SendPlayedCard, cardType);
        
        if (_gameBoard.GetPlayedCardCount() == 4) {
            _gameBoard.ClearMiddle(); 
        }
        _gameBoard.AddCardToMiddle((CardType) cardType);
    }

    [Rpc]
    private void _SendPlayedCard(int cardType) {
        _playedCards.Add((CardType) cardType);
    }

    [Rpc(CallLocal = true)]
    private void _ClearPlayedCards() {
        _playedCards.Clear();
    }

    [Rpc(CallLocal = true)]
    private void _AwardTrick(int id) {
        
    }
}

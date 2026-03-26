using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames.Schafkopf;

public partial class Schafkopf : TurnBasedMultiplayerGame {
    
    [Export] private GameBoard _gameBoard;

    private List<long> _otherPlayers = new(3);
    private List<CardType> _playedCards = new(4);
    private SchafkopfLogic _schafkopfLogic = new();


    public override void _Ready() {
        base._Ready();
        if (Multiplayer.IsServer()) {
            GetTree().CreateTimer(1).Timeout += GameReady;
        }
    }


    public override int GetMaxPlayerCount() {
        return 4;
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
            
            return nextPlayer;
        } else {
            return base.DetermineNextPlayer();
        }
    }
    
    private void GameReady() {
        if (MultiplayerConnection.CurrentPlayerCount == MultiplayerConnection.MaxPlayerCount) {
            // todo does not work yet
            MultiplayerConnection.TransmitData();
            Rpc(MethodName.TransmitPlayerOrder, PeerOrder.ToArray());
            Rpc(MethodName.InitGame); 
            StartGame();
            Rpc(MethodName.AnnounceNextPlayer, PeerOrder[0]); 
        } else {
            GetTree().CreateTimer(1).Timeout += GameReady;
        }
    }

    private void StartGame() {
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
    }
    
    [Rpc(CallLocal = true)]
    private void InitGame() {
        _otherPlayers.Clear();
        int index = PeerOrder.IndexOf(Multiplayer.GetUniqueId());
        var otherPlayersNode = GetNode("GameBoard/OtherPlayers");
        for (int i = 1; i < GetMaxPlayerCount(); i++) {
            long id = PeerOrder[(i + index) % GetMaxPlayerCount()];
            _otherPlayers.Add(id);
            var name = MultiplayerConnection.GetNameOfId(id);
            GD.Print($"{name} for {id} on {Multiplayer.GetUniqueId()}");
            otherPlayersNode.GetChild<OtherPlayer>(i - 1).SetPlayerName(MultiplayerConnection.GetNameOfId(id));
        }
    }
    
    void OnTurnChanged(bool isMyTurn) {
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

    void PlayCard(Card card) {
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
    private void _AwardTrick(int id) {
        var ownId = Multiplayer.GetUniqueId();
        if (id == ownId) {
            _gameBoard.AwardTrickToSelf(_playedCards.ToArray());
        } else {
            int index = _otherPlayers.IndexOf(id);
            _gameBoard.AwardTrick(index, _playedCards.ToArray());
        }
        _playedCards.Clear();
    }
}

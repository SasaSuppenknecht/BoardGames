using System.Linq;
using Godot;
using BoardGames.Schafkopf;
using Godot.Collections;

public partial class GameBoard : Node {
    [Signal] public delegate void CardPlayedEventHandler(Card card);
    
    [Export] private Container _deck;
    [Export] private Container _middle;
    [Export] private PackedScene _cardScene;

    private bool _isTurn = false;
    
    public void OnCardPressed(Card card) {
        if (_isTurn) {
            card.CardPressed -= OnCardPressed;
            _deck.RemoveChild(card);
            EmitSignalCardPlayed(card);
        }
    }

    public void AddCardToDeck(CardType cardType) {
        Card card = _cardScene.Instantiate<Card>();
        card.Type = cardType;
        _deck.AddChild(card, true);
    }

    public void AddCardToMiddle(CardType cardType) {
        Card card = _cardScene.Instantiate<Card>();
        card.Type = cardType;
        _middle.AddChild(card, true);
    }

    public void ClearMiddle() {
        foreach (Node node in _middle.GetChildren()) {
            _middle.RemoveChild(node);
        }
    }

    public int GetPlayedCardCount() {
        return _middle.GetChildCount();
    }

    public Card[] GetCardsInDeck() {
        return _deck.GetChildren().Select(node => (Card) node).ToArray();
    }

    public void OnTurnChanged(bool isTurn) {
        _isTurn = isTurn;
    }

    public void AwardTrick(int index) {
        
    }

    public void AwardTrickToSelf() {
        
    }
    
    // node added to _deck
    private void OnNodeAdded(Node node) {
        if (node is Card card) {
            card.CardPressed += OnCardPressed;
        }
    }

}

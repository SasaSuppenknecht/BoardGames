using Godot;
using System;

public partial class Schafkopf : Control {
    
    [Export] private Container _deck;
    [Export] private Container _middle;
    [Export] private SchafkopfLogic _schafkopfLogic;

    private bool _isTurn = false;

    public void OnTurnBegins() {
        _isTurn = true;
    }

    public void OnCardPressed(Card card) {
        if (_isTurn) {
            card.CardPressed -= OnCardPressed;
            _deck.RemoveChild(card);
            _schafkopfLogic.PlayCard((int) card.Type);
            _isTurn = false;
            _schafkopfLogic.EndTurn();
        }
    }
}

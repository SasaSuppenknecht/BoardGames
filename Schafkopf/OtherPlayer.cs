using Godot;
using System;
using BoardGames.Schafkopf;

public partial class OtherPlayer : HighlightablePanelContainer {
    
    private int _currentIndex = -1;
    private int _trickCount = 0;
    
    public void SetPlayerName(string name) {
        GetNode<Label>("%PlayerName").Text = name;
    }

    public void AddTrick(CardType[] cards) {
        _trickCount++;
        GetNode<Label>("%TrickCount").Text = $"{_trickCount}";
        
        var trickCardsNode = GetNode<TrickCards>("%TrickCards");
        trickCardsNode.Visible = true;
        trickCardsNode.SetCards(cards);
    }

    public void PlayCard() {
        var deck= GetNode("%Deck");
        deck.GetChild<Control>(_currentIndex).Visible = false;
        _currentIndex--;
    }

    public void Reset() {
        var deck = GetNode("%Deck");
        foreach (Control card in deck.GetChildren()) {
            card.Visible = true;
        }
        
        GetNode<TrickCards>("%TrickCards").Visible = false;
        _currentIndex = -1;
        _trickCount = 0;
    }
    
}

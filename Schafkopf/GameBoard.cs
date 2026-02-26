using Godot;
using System;

public partial class GameBoard : Node {

    private void OnNodeAdded(Node node) {
        if (node is Card card) {
            card.CardPressed += GetParent<Schafkopf>().OnCardPressed;
        }
    }
    
}

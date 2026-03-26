using Godot;
using System;
using BoardGames.Schafkopf;

public partial class TrickCards : TextureRect {
    
    public void SetCards(CardType[] cardTypes) {
        var popupWindow = GetNode("Window/Panel/MarginContainer/HBoxContainer");
        
        for (int i = 0; i < popupWindow.GetChildCount(); i++) {
            var card = popupWindow.GetChild<Card>(i);
            card.Type = cardTypes[i];
        }
    }
    
    private void OnGuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }) {
            GetNode<Popup>("Window").PopupCentered();
        }
    }
    
    
}

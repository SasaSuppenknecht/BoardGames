using Godot;
using System;

public partial class JoinMenu : VBoxContainer {

    [Export] private LineEdit _ip;
    [Export] private LineEdit _port;

    [Export] private Button _back;
    [Export] private Button _join;

    [Export] private MainMenu _mainMenu;

    public override void _Ready() {
        _back.Pressed += () => {
            _mainMenu.Show();
            Hide();
        };
    }
}

using Godot;
using System;

public partial class HostMenu : VBoxContainer {

    [Export] private Button _back;
    [Export] private Button _start;
    
    [Export] private MainMenu _mainMenu;

    public override void _Ready() {
        _back.Pressed += () => {
            _mainMenu.Show();
            Hide();
        };
    }
}

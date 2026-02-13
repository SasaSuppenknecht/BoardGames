using Godot;
using System;

public partial class MainMenu : VBoxContainer {

    [Export] private Button _host;
    [Export] private Button _join;
    
    [Export] private JoinMenu _joinMenu;
    [Export] private HostMenu _hostMenu;

    public override void _Ready() {
        _join.Pressed += () => {
            _joinMenu.Show();
            Hide();
        };
        _host.Pressed += () => {
            _hostMenu.Show();
            Hide();
        };
    }
}

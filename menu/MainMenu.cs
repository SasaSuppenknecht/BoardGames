using Godot;
using System;

public partial class MainMenu : VBoxContainer {
    
    [Export] private JoinMenu _joinMenu;
    [Export] private HostMenu _hostMenu;

    public override void _Ready() {
        GetNode<Button>("%Join").Pressed += () => {
            _joinMenu.Show();
            Hide();
        };
        GetNode<Button>("%Host").Pressed += () => {
            _hostMenu.Show();
            Hide();
        };
    }
}

using Godot;
using System;

public partial class HostMenu : VBoxContainer {
    
    [Export] private Button _back;
    [Export] private Button _start;
    
    [Export] private MainMenu _mainMenu;
    [Export] private PackedScene[] _games;

    public override void _Ready() {
        _back.Pressed += () => {
            _mainMenu.Show();
            Hide();
        };

        var gameList = GetNode("%GameList");
        var root = GetTree().Root;
        var connectionInfo = GetNode<ConnectionInfo>("%ConnectionInfo");
        var menus = (Control) GetParent();
        for (int i = 0; i < _games.Length; i++) {
            var game = _games[i];
            Label label = new Label();
            gameList.AddChild(label);
            string resourcePath = game.ResourcePath;
            int slashIndex = resourcePath.LastIndexOf('/');
            int dotIndex = resourcePath.LastIndexOf('.');
            string name = resourcePath.Substring(slashIndex + 1, dotIndex - slashIndex - 1);
            label.Text = name;
            label.MouseFilter = MouseFilterEnum.Stop;
            label.GuiInput += @event => {
                if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
                    var gameNode = game.Instantiate();
                    root.AddChild(gameNode);
                    root.MoveChild(gameNode, 0);
                    connectionInfo.UpdateInfo(2000);
                    menus.Hide();
                }
            };
        }
    }
}

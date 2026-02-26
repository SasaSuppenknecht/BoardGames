using Godot;
using System;

public partial class HostMenu : VBoxContainer {
    
    [Export] private Button _back;
    [Export] private Button _start;

    [Export] private LineEdit _player;
    [Export] private LineEdit _port;
    
    [Export] private MainMenu _mainMenu;
    [Export] private PackedScene[] _games;

    public override void _Ready() {
        _back.Pressed += () => {
            _mainMenu.Show();
            Hide();
        };

        var gameList = GetNode("%GameList");
        var sceneRoot = GetOwner();
        var connectionInfo = GetNode<Label>("%ConnectionInfo");
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
                    string portText = _port.Text;
                    if (!int.TryParse(portText, out int port) || port < 2000 || port > 65535) {
                        return;
                    }
                    
                    string player = _player.Text;
                    if (player.Trim().Length == 0) {
                        return;
                    }
                    
                    var gameNode = game.Instantiate();
                    sceneRoot.AddChild(gameNode, true);
                    sceneRoot.MoveChild(gameNode, 0);
                    menus.Hide();
                }
            };
        }
    }
}

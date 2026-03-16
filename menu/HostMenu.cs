using Godot;
using System;

public partial class HostMenu : VBoxContainer {
    
    [Export] private MainMenu _mainMenu;
    [Export] private PackedScene[] _games;
    [Export] private MultiplayerConnection _multiplayerConnection;

    public override void _Ready() {
        GetNode<Button>("%Back").Pressed += () => {
            _mainMenu.Show();
            Hide();
        };

        var gameList = GetNode("%GameList");
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
                    string portText = GetNode<LineEdit>("%PortLine").Text;
                    if (!int.TryParse(portText, out int port) || port < 2000 || port > 65535) {
                        return;
                    }
                    
                    string player = GetNode<LineEdit>("%PlayerLine").Text;
                    if (player.Trim().Length == 0) {
                        return;
                    }
                    
                    var gameNode = game.Instantiate<TurnBasedMultiplayerGame>();
                    var sceneRoot = GetOwner();
                    sceneRoot.AddChild(gameNode, true);
                    sceneRoot.MoveChild(gameNode, 0);
                    _multiplayerConnection.SetupServer(player, gameNode.GetMaxPlayerCount(), port);
                    var menus = (Control) GetParent();
                    menus.Hide();
                }
            };
        }
    }
}

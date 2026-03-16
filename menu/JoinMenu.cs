using Godot;
using System.Text.RegularExpressions;

public partial class JoinMenu : VBoxContainer {

    [Export] private MainMenu _mainMenu;
    [Export] private MultiplayerConnection _multiplayerConnection;

    private string _ipPattern = @"(?:\d{1,3}.){3}\d{1,3}|localhost";

    public override void _Ready() {
        GetNode<Button>("%Back").Pressed += () => {
            _mainMenu.Show();
            Hide();
        };
    }

    private void JoinGame() {
        var _ip = GetNode<LineEdit>("IPLine");
        string ip = _ip.Text;
        ip = ip.Trim();
        Regex regex = new Regex(_ipPattern);
        Match match = regex.Match(ip);
        if (!match.Success || match.Value.Length != ip.Length) {
            return;
        }
        
        var _port = GetNode<LineEdit>("PortLine");
        string portText = _port.Text;
        if (!int.TryParse(portText, out int port) || port < 2000 || port > 65535) {
            return;
        }

        var _player = GetNode<LineEdit>("PlayerLine");
        string player = _player.Text;
        if (player.Trim().Length == 0) {
            return;
        }
        
        _multiplayerConnection.SetupClient(player, ip, port);
        
        ((Control)GetParent()).Hide();
    }
}

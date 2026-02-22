using Godot;
using System;
using System.Text.RegularExpressions;

public partial class JoinMenu : VBoxContainer {

    [Export] private LineEdit _ip;
    [Export] private LineEdit _port;
    [Export] private LineEdit _player;

    [Export] private Button _back;
    [Export] private Button _join;

    [Export] private MainMenu _mainMenu;

    private string _ipPattern = @"(\d{1,3}.){3}\d{1,3}";

    public override void _Ready() {
        _back.Pressed += () => {
            _mainMenu.Show();
            Hide();
        };
    }

    private void JoinGame() {
        string ip = _ip.Text;
        ip = ip.Trim();
        Regex regex = new Regex(_ipPattern);
        Match match = regex.Match(ip);
        if (!match.Success || match.Value.Length != ip.Length) {
            return;
        }
            
        string portText = _port.Text;
        if (!int.TryParse(portText, out int port) || port < 2000 || port > 65535) {
            return;
        }

        string player = _player.Text;
        if (player.Trim().Length == 0) {
            return;
        }

        GlobalData.Instance.PlayerName = player;
        GlobalData.Instance.IP = ip;
        GlobalData.Instance.Port = port;

        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(ip, port);
        if (error != Error.Ok) {
            return;
        }
        GetTree().Root.GetMultiplayer().MultiplayerPeer = peer;
        ((Control)GetParent()).Hide();
    }
}

using Godot;
using System;
using System.Linq;

public partial class TicTacToe : Control {
    public static readonly int Server = 1;
    public static string Player;
    public static string OtherPlayer;
    
    private const string Ip = "192.168.10.20";
    private const int Port = 8000;

    private GameState _gameState;
    private long _secondPlayerId;
    
    [Export] private Label _info;
    private Board _board;
    private Field[] fields = new Field[9];
    
    
    public override void _Ready() {
        _board = GetNode<Board>("%Board");
        var nodes = _board.GetChildren();
        for (int index = 0; index < nodes.Count; index++) {
            var node = (Field) nodes[index];
            fields[index] = node;
            node.FieldUpdated += OnFieldUpdated;
        }

        var args = OS.GetCmdlineArgs();

        var peer = new ENetMultiplayerPeer();
        var multiplayer = GetTree().GetMultiplayer();
        if (args.Contains("--server")) {
            peer.CreateServer(Port, 2);
            Player = "X";
            OtherPlayer = "O";
            _gameState = new();
            
            Multiplayer.PeerConnected += id => {
                _secondPlayerId = id;
                InitGame();
            };
        } else {
            peer.CreateClient(Ip, Port);
            Player = "O";
            OtherPlayer = "X";
        }
        multiplayer.MultiplayerPeer = peer;
    }

    public void OnRematchButtonToggled(bool toggledOn) {
        RpcId(Server, MethodName.UpdateRematch, toggledOn);
    }

    // --- Server only code ---
    
    public void OnFieldUpdated(int id, Field field) {
         int index = Array.IndexOf(fields, field);
         _gameState.Board[index] = id == Server ? 1 : -1;
         
         WinState win = CheckWin();
         if (win == WinState.Continue) {
             if (id == Server) {
                 UpdateTurn(false);
                 RpcId(_secondPlayerId, MethodName.UpdateTurn, true);
             } else {
                 UpdateTurn(true);
                 RpcId(_secondPlayerId, MethodName.UpdateTurn, false);
             }
             return;
         }

         UpdateTurn(false);
         RpcId(_secondPlayerId, MethodName.UpdateTurn, false);
         if (win == WinState.XWins) {
             UpdateWin("You won!");
             RpcId(_secondPlayerId, MethodName.UpdateWin, "You lost!");
         } else if (win == WinState.OWins) {
             UpdateWin("You won!");
             RpcId(_secondPlayerId, MethodName.UpdateWin, "You won!");
         } else {
             UpdateWin("Draw!");
             RpcId(_secondPlayerId, MethodName.UpdateWin, "Draw!");
         }
    }

    private WinState CheckWin() {
        var board = _gameState.Board;

        (int, int, int)[] combinations = [
            (0, 1, 2), (3, 4, 5), (6, 7, 8), (0, 3, 6), (1, 4, 7), (2, 5, 8), (0, 4, 8), (2, 4, 6)
        ];
        
        foreach (var combination in combinations) {
            int first = board[combination.Item1];
            if (first == 0) continue;
            
            int second = board[combination.Item2];
            int third = board[combination.Item3];
            if (first == second && second == third) {
                return first == 1 ? WinState.XWins : WinState.OWins;
            }
        }

        if (board.Contains(0)) {
            return WinState.Continue;
        } else {
            return WinState.Draw;
        }
    }
    
    private void InitGame() {
        foreach (var field in fields) {
            field.Content = "";
            field.Clickable = true;
        }

        _gameState.IsHostTurn = !_gameState.HostStartedLastRound;

        UpdateTurn(_gameState.IsHostTurn);
        RpcId(_secondPlayerId, MethodName.UpdateTurn, !_gameState.IsHostTurn);
        
        _gameState.HostStartedLastRound = !_gameState.HostStartedLastRound;
        Array.Fill(_gameState.Board, 0); 
    }

    [Rpc(CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)] 
    private void UpdateTurn(bool yourTurn) {
        if (yourTurn) {
            _info.Text = "Your Turn!";
            _board.Enabled = true;
        } else {
            _info.Text = $"Turn of Player {OtherPlayer}";
            _board.Enabled = false;
        }
    }
    
    [Rpc(CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)] 
    private void UpdateWin(string text) {
        _info.Text = text;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdateRematch(bool rematchToggled) {
        var senderId = Multiplayer.GetRemoteSenderId();
        if (senderId == Server) {
            _gameState.HostRematch = rematchToggled;
        } else {
            _gameState.ClientRematch = rematchToggled;
        }

        if (_gameState.HostRematch && _gameState.ClientRematch) {
            _gameState.HostRematch = false;
            _gameState.ClientRematch = false;
            ResetRematchButton();
            RpcId(_secondPlayerId, MethodName.ResetRematchButton);
            InitGame();
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ResetRematchButton() {
        GetNode<Button>("%Rematch").ButtonPressed = false;
    }
    
    private struct GameState() {
        public bool IsHostTurn;
        public bool HostStartedLastRound = false;
        public readonly int[] Board = new int[9];
        public bool HostRematch = false;
        public bool ClientRematch = false;
    }

    enum WinState {
        Continue, XWins, OWins, Draw
    }
}



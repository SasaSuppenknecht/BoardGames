using Godot;
using System;


public partial class Field : PanelContainer {

    [Signal]
    public delegate void FieldUpdatedEventHandler(int id, Field field);

    [Export] public string Content {
        get => _label.Text;
        set => _label.Text = value;
    }

    [Export ]public bool Clickable {
        get => _label.MouseFilter == MouseFilterEnum.Stop;
        set {
            if (value) {
                _label.MouseFilter = MouseFilterEnum.Stop;
            } else {
                _label.MouseFilter = MouseFilterEnum.Ignore;
            }
        }
    }

    private Label _label;

    public override void _Ready() {
        _label = GetNode<Label>("Label");

        _label.GuiInput += OnMouseButtonPressed;
    }

    private void OnMouseButtonPressed(InputEvent @event) {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
            RpcId(TicTacToe.Server, MethodName.UpdateField);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void UpdateField() {
        var senderId = Multiplayer.GetRemoteSenderId();
        Content = senderId == TicTacToe.Server? TicTacToe.Player : TicTacToe.OtherPlayer;
        Clickable = false;
        EmitSignal(SignalName.FieldUpdated, senderId, this);
    }
}

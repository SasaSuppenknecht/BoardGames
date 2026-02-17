using Godot;
using System;

public partial class ConnectionInfo : Label {

    private string _ip;
    
    public override void _GuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
            DisplayServer.ClipboardSet(_ip);
        }
    }
    
    public async void UpdateInfo(int port) {
        var requestNode = GetChild<HttpRequest>(0);
        requestNode.Request("https://api.ipify.org", []);
        await ToSignal(requestNode, HttpRequest.SignalName.RequestCompleted);
        Text += $"\nPort: {port}";
        Show();
    }

    private void requestCompleted(int result, int responseCode, string[] headers, byte[] body) {
        if (responseCode == 200) {
            string message = body.GetStringFromUtf8();
            _ip = message;
            Text = "IP: " + message;
        } else {
            GD.PushError("Request failed");
            Callable.From(() => Text = "IP: -\nPort: -").CallDeferred();
        }
    }
    
}

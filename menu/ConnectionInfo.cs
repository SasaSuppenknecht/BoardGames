using Godot;
using System;

public partial class ConnectionInfo : Label {
    
    public override void _GuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
            DisplayServer.ClipboardSet(GlobalData.Instance.IP);
        }
    }
    
    public async void UpdateInfo() {
        var requestNode = GetChild<HttpRequest>(0);
        requestNode.Request("https://api.ipify.org", []);
        await ToSignal(requestNode, HttpRequest.SignalName.RequestCompleted);
        Show();
    }

    private void requestCompleted(int result, int responseCode, string[] headers, byte[] body) {
        if (responseCode == 200) {
            string message = body.GetStringFromUtf8();
            GlobalData.Instance.IP = message;
            Text = $"IP:{message}\nPort: {GlobalData.Instance.Port}";
        } else {
            GD.PushError("Request failed");
            Callable.From(() => Text = "IP: -\nPort: -").CallDeferred();
        }
    }
    
}

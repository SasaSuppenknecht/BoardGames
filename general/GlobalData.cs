using Godot;
using System;

public partial class GlobalData : Node {
    public const int COMM_CHANNEL = 4;
    
    public static GlobalData Instance { get; private set; }
    
    public bool IsHost = false;
    public string PlayerName;
    public string IP = "";
    public int Port = -1;

    public override void _Ready() {
        Instance = this;
    }
}

using Godot;
using System;

public partial class Board : GridContainer {
    
    public bool Enabled {
        set {
            if (value) {
                foreach (var node in GetChildren()) {
                    var field = (Field)node;
                    field.MouseBehaviorRecursive = MouseBehaviorRecursiveEnum.Enabled;
                }
            } else {
                foreach (var node in GetChildren()) {
                    var field = (Field)node;
                    field.MouseBehaviorRecursive = MouseBehaviorRecursiveEnum.Disabled;
                }
            }
        }
    }
    
    
}

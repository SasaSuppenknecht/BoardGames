using Godot;
using System;

public partial class HighlightablePanelContainer : PanelContainer {

    [Export] public bool Highlighted {
        get => _highlighted;
        set {
            _highlighted = value;
            var styleBox = (StyleBoxFlat) GetThemeStylebox("panel").Duplicate();
            
            int borderWidth = value ? 2 : 0;
            
            styleBox.BorderWidthBottom = borderWidth;
            styleBox.BorderWidthLeft = borderWidth;
            styleBox.BorderWidthRight = borderWidth;
            styleBox.BorderWidthTop = borderWidth;
            
            AddThemeStyleboxOverride("panel", styleBox);
        }
    }
    
    private bool _highlighted = false;
    
}

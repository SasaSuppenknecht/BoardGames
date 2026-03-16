using Godot;
using BoardGames.Schafkopf;
using Microsoft.VisualBasic.CompilerServices;

[Tool]
public partial class Card : TextureRect {
    private const int AtlasColumns = 10;

    [Signal] public delegate void CardPressedEventHandler(Card card);
    
    /// <summary>
    /// Selecting a card type will change the texture accordingly.
    /// </summary>
    /// <exception cref="IncompleteInitialization">Raised when texture is not a <see cref="AtlasTexture"/></exception>
    [Export(PropertyHint.Enum)]
    public CardType Type {
        get => _cardType;
        set {
            _cardType = value;
            var atlasTexture = Texture as AtlasTexture; // throw new IncompleteInitialization();
            int row = (int) _cardType / AtlasColumns;
            int col = (int) _cardType % AtlasColumns;
            var size = atlasTexture.Region.Size;
            atlasTexture.Region = new Rect2(size.X * col, size.Y * row, size.X, size.Y);
        }
    }
    private CardType _cardType;
    
    [Export]
    public bool Clickable {
        get => _clickable;
        set {
            _clickable = value;
            if (value) {
                MouseFilter = MouseFilterEnum.Stop;
                Modulate = new Color(1, 1, 1, 1);
            } else {
                MouseFilter = MouseFilterEnum.Ignore;
                Modulate = new Color(0.4f, 0.4f, 0.4f);
            }
        }
    }

    private bool _clickable = true;
    

    public override void _GuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) {
            EmitSignalCardPressed(this);
        }
    }

}

using Godot;

namespace Engine.Scripts;

[GlobalClass]
public partial class GridSettings : Resource
{
    private Vector2 _gridDimensions;

    public Vector2 GridDimensions
    {
        get => _gridDimensions;
        set
        {
            _gridDimensions = value;
            CellSize = _gridDimensions / GridSize;
            Margin = (Vector2I)(CellSize / 2).Round();
        }
    }

    [Export(PropertyHint.Range, "2,19,1")]
    public int GridSize { get; set; } = 19;

    [Export]
    public float HoshiRadius { get; set; } = 4f;

    [Export]
    public float LineWidth { get; set; } = -1f;

    [Export]
    public Color LineColor { get; set; } = Colors.Black;

    [Export]
    public bool Antialiased { get; set; }

    [Export]
    public int FontSize { get; set; }

    [Export(PropertyHint.Enum)]
    public CoordinatePlacement CoordinatePlacement { get; set; }

    public Vector2 CellSize { get; private set; }
    public Vector2I Margin { get; private set; }
}
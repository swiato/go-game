using Godot;

namespace Engine.Scripts;

[Tool]
public partial class Stone : Control
{
    // https://senseis.xmp.net/?EquipmentDimensions
    [Export]
    public float StoneSize { get; set; }
    [Export]
    public Color Color { get; set; } = Colors.Black;
    [Export]
    private Color _lineColor = Colors.Black;
    [Export]
    private float _lineWidth = -1;
    [Export]
    private bool _antialiased = false;

    public override void _Draw()
    {
        float radius = (StoneSize - _lineWidth) / 2f;
        DrawCircle(Vector2.Zero, radius, Color, true);
        DrawCircle(Vector2.Zero, radius, _lineColor, false, _lineWidth, _antialiased);
    }
}

using Godot;

namespace Engine.Scripts;

public partial class GridSlow : Control
{
    [Export] private GridSettings _gridSettings;

    public override void _Draw()
    {
        GD.Print("Draw Grid");
        DrawBoard();
    }

    private void DrawBoard()
    {
        DrawHorizontalLines();
        DrawVerticalLines();
        DrawHoshiPoints();
    }

    private void DrawHorizontalLines()
    {
        for (int y = 0; y < _gridSettings.GridSize; y++)
        {
            Vector2 from = new Vector2(0f, y) * _gridSettings.CellSize;
            Vector2 to = new(Size.X, from.Y);


            DrawGridLine(from, to);
        }
    }

    private void DrawVerticalLines()
    {
        for (int x = 0; x < _gridSettings.GridSize; x++)
        {
            Vector2 from = new Vector2(x, 0f) * _gridSettings.CellSize;
            Vector2 to = new(from.X, Size.Y);
            DrawGridLine(from, to);
        }
    }

    private void DrawGridLine(Vector2 from, Vector2 to)
    {
        DrawLine(from, to, _gridSettings.LineColor, _gridSettings.LineWidth, _gridSettings.Antialiased);
    }

    private void DrawHoshiPoints()
    {
        Vector2I[] hoshiPoints = GetHoshiPoints();

        for (int i = 0; i < hoshiPoints.Length; i++)
        {
            Vector2 hoshiPoint = hoshiPoints[i] * _gridSettings.CellSize;
            DrawHoshiPoint(hoshiPoint);
        }
    }

    private void DrawHoshiPoint(Vector2 hoshiPoint)
    {
        DrawCircle(hoshiPoint, _gridSettings.HoshiRadius, _gridSettings.LineColor, true, _gridSettings.LineWidth, _gridSettings.Antialiased);
    }

    // https://senseis.xmp.net/?Hoshi
    private Vector2I[] GetHoshiPoints()
    {
        if (_gridSettings.GridSize > 13)
        {
            return
            [
                new Vector2I(3, 3),
                new Vector2I(9, 3),
                new Vector2I(15, 3),

                new Vector2I(3, 9),
                new Vector2I(9, 9),
                new Vector2I(15, 9),

                new Vector2I(3, 15),
                new Vector2I(9, 15),
                new Vector2I(15, 15),
            ];
        }
        else if (_gridSettings.GridSize > 9)
        {
            return
            [
                new Vector2I(3, 3),
                new Vector2I(9, 3),

                new Vector2I(3, 9),
                new Vector2I(9, 9),

                new Vector2I(6, 6),
            ];
        }
        else
        {
            return
            [
                new Vector2I(2, 2),
                new Vector2I(6, 2),

                new Vector2I(2, 6),
                new Vector2I(6, 6),

                new Vector2I(4, 4),
            ];
        }
    }
}
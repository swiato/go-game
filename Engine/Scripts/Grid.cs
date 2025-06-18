using Godot;
using Domain.Go;

namespace Engine.Scripts;

[Tool]
public partial class Grid : Control
{
    private static readonly string[] AlphabeticCoordinates = ["A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T"];
    private static readonly string[] NumericCoordinates = ["19", "18", "17", "16", "15", "14", "13", "12", "11", "10", "9", "8", "7", "6", "5", "4", "3", "2", "1"];

    public Vector2 CellSize { get; private set; }
    private Vector2 _coordinatePadding;
    private Vector2 _gridDimensions;
    private Vector2 _gridOffset;

    [Export(PropertyHint.Range, "2,19,1")]
    public int GridSize { get; set; } = 19;

    [Export]
    private float _hoshiRadius = 4f;

    [Export]
    private float _lineWidth = -1f;

    [Export]
    private Color _lineColor { get; set; } = Colors.Black;

    [Export]
    private bool _antialiased;

    [Export]
    private Font _font;

    [Export]
    private int _fontSize = 24;

    [Export]
    private CoordinatePlacement _coordinatePlacement;

    public override void _Ready()
    {
        InitializeGrid();
    }

    public override void _Draw()
    {
        GD.Print("Draw Grid");
        DrawGrid();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
        {
            Vector2 local = eventMouseButton.Position - _gridOffset;
            local /= CellSize;
            Vector2I coordinate = (Vector2I)local.Round();

            Point point = new(Column: coordinate.X, Row: coordinate.Y);
            GameEvents.OnPlayed(this, new(Move.Play(point)));
        }
    }

    public Vector2 PointToPosition(Point point)
    {
        Vector2I coordinate = new(point.Column, point.Row);
        return _gridOffset + coordinate * CellSize;
    }

    private void InitializeGrid()
    {
        _gridOffset = GetGridOffset();
        _gridDimensions = Size - (_gridOffset * 2);
        CellSize = _gridDimensions / (GridSize - 1);
        _coordinatePadding = CellSize / 1.5f;
    }

    private void DrawGrid()
    {
        DrawHorizontalLines();
        DrawVerticalLines();
        DrawHoshiPoints();
        DrawHorizontalCoordinates();
        DrawVerticalCoordinates();
    }

    private void DrawHorizontalLines()
    {
        Vector2 cellHeight = Vector2.Down * CellSize;
        Vector2 gridWidth = Vector2.Right * _gridDimensions;

        for (int y = 0; y < GridSize; y++)
        {
            Vector2 from = _gridOffset + cellHeight * y;
            Vector2 to = from + gridWidth;

            DrawGridLine(from, to);
        }
    }

    private void DrawVerticalLines()
    {
        Vector2 cellWidth = Vector2.Right * CellSize;
        Vector2 gridHeight = Vector2.Down * _gridDimensions;

        for (int x = 0; x < GridSize; x++)
        {
            Vector2 from = _gridOffset + cellWidth * x;
            Vector2 to = from + gridHeight;

            DrawGridLine(from, to);
        }
    }

    private void DrawGridLine(Vector2 from, Vector2 to)
    {
        DrawLine(from, to, _lineColor, _lineWidth, _antialiased);
    }

    private void DrawHoshiPoints()
    {
        Vector2I[] hoshiPoints = GetHoshiPoints();

        for (int i = 0; i < hoshiPoints.Length; i++)
        {
            Vector2 hoshiPoint = _gridOffset + hoshiPoints[i] * CellSize;
            DrawHoshiPoint(hoshiPoint);
        }
    }

    private void DrawHoshiPoint(Vector2 hoshiPoint)
    {
        DrawCircle(hoshiPoint, _hoshiRadius, _lineColor);
    }

    private void DrawHorizontalCoordinates()
    {
        DrawTopCoordinate();
        DrawBottomCoordinate();
    }

    private void DrawTopCoordinate()
    {
        if (_coordinatePlacement.HasFlag(CoordinatePlacement.Top))
        {
            Vector2 startPosition = _gridOffset;
            Vector2 step = Vector2.Right * CellSize;
            Vector2 textPosition = Vector2.Left / 2;
            Vector2 coordinatePosition = Vector2.Up;

            DrawCoordinates(AlphabeticCoordinates, startPosition, step, textPosition, coordinatePosition);
        }
    }

    private void DrawBottomCoordinate()
    {
        if (_coordinatePlacement.HasFlag(CoordinatePlacement.Bottom))
        {
            Vector2 gridHeight = Vector2.Down * _gridDimensions;
            Vector2 startPosition = _gridOffset + gridHeight;
            Vector2 step = Vector2.Right * CellSize;
            Vector2 textPosition = Vector2.Left / 2 + Vector2.Down / 2;
            Vector2 coordinatePosition = Vector2.Down;

            DrawCoordinates(AlphabeticCoordinates, startPosition, step, textPosition, coordinatePosition);
        }
    }

    private void DrawVerticalCoordinates()
    {
        DrawLeftCoordinate();
        DrawRightCoordinate();
    }

    private void DrawLeftCoordinate()
    {
        if (_coordinatePlacement.HasFlag(CoordinatePlacement.Left))
        {
            Vector2 startPosition = _gridOffset;
            Vector2 step = Vector2.Down * CellSize;
            Vector2 textPosition = Vector2.Down / 4 + Vector2.Left;
            Vector2 coordinatePosition = Vector2.Left;

            DrawCoordinates(NumericCoordinates[^GridSize..], startPosition, step, textPosition, coordinatePosition);
        }
    }

    private void DrawRightCoordinate()
    {
        if (_coordinatePlacement.HasFlag(CoordinatePlacement.Right))
        {
            Vector2 gridWidth = Vector2.Right * _gridDimensions;
            Vector2 startPosition = _gridOffset + gridWidth;
            Vector2 step = Vector2.Down * CellSize;
            Vector2 textPosition = Vector2.Down / 4;
            Vector2 coordinatePosition = Vector2.Right;

            DrawCoordinates(NumericCoordinates[^GridSize..], startPosition, step, textPosition, coordinatePosition);
        }
    }

    private void DrawCoordinates(string[] coordinates, Vector2 start, Vector2 step, Vector2 textPosition, Vector2 coordinatePosition)
    {
        for (int i = 0; i < GridSize; i++)
        {
            string coordinate = coordinates[i];
            Vector2 textSize = _font.GetStringSize(coordinate, HorizontalAlignment.Center, -1, _fontSize);
            Vector2 textOffset = textSize * textPosition;
            Vector2 coordinateOffset = coordinatePosition * _coordinatePadding;

            Vector2 position = start + step * i + textOffset + coordinateOffset;

            DrawString(_font, position, coordinate, HorizontalAlignment.Center, -1, _fontSize, _lineColor);
        }
    }

    private Vector2 GetGridOffset()
    {
        if (_coordinatePlacement == CoordinatePlacement.None)
        {
            return Size / GridSize / 1.5f;
        }

        if (GridSize > 13)
        {
            return Size / GridSize * 1.4f;
        }

        if (GridSize > 9)
        {
            return Size / GridSize * 1.2f;
        }

        return Size / GridSize;
    }

    // https://senseis.xmp.net/?Hoshi
    private Vector2I[] GetHoshiPoints()
    {
        if (GridSize > 13)
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

        if (GridSize > 9)
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
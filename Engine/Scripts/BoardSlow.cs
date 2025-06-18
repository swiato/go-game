using Godot;
using System;
using System.Linq;

namespace Engine.Scripts;

public partial class BoardSlow : Control
{
    [Export] private GridSettings _gridSettings;
    [Export] private LabelSettings _labelSettings;
    [Export] private MarginContainer _playArea;
    [Export] private GridSlow _grid;
    [Export] private Control _stones;
    [Export] private PackedScene _stoneScene = GD.Load<PackedScene>("res://Scenes/Stone/Stone.tscn");
    [Export] private Label _leftCoords;
    [Export] private Label _rightCoords;
    [Export] private Label _topCoords;
    [Export] private Label _bottomCoords;

    const string AlphaCoordinates = "ABCDEFGHJKLMNOPQRST";

    public override void _Ready()
    {
        UpdateFont();
        UpdateCoords();

        _playArea.Draw += OnPlayAreaDraw;
        _playArea.GuiInput += OnPlayAreaGuiInput;
    }

    public override void _ExitTree()
    {
        _playArea.Draw -= OnPlayAreaDraw;
        _playArea.GuiInput -= OnPlayAreaGuiInput;
    }

    private void OnPlayAreaGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
        {
            Vector2 local = eventMouseButton.Position - _grid.Position;
            local /= _gridSettings.CellSize;
            Vector2I point = (Vector2I)local.Round();
            point = point.Clamp(0, _gridSettings.GridSize - 1);
            PlaceStone(point);
        }
    }

    private void PlaceStone(Vector2I point)
    {
        Stone stone = _stoneScene.Instantiate<Stone>();
        // stone.StoneSize = Mathf.Min(_gridSettings.CellSize.X, _gridSettings.CellSize.Y);
        stone.StoneSize = _gridSettings.CellSize.X;
        stone.Position = point * _gridSettings.CellSize;
        _stones.AddChild(stone);
    }

    private void OnPlayAreaDraw()
    {
        GD.Print("OnGridDraw");

        UpdateGrid();
        UpdatePlayArea();
        UpdateCoordsMargins();
    }

    private void UpdateFont()
    {
        _labelSettings.FontSize = _gridSettings.FontSize;
        _labelSettings.FontColor = _gridSettings.LineColor;
    }

    private void UpdateCoords()
    {
        SetHorizontalCoords();
        SetVerticalCoords();
    }

    private void UpdateCoordsMargins()
    {
        SetHorizontalCoordsMargins();
        SetVerticalCoordsMargins();
    }

    private void SetHorizontalCoordsMargins()
    {
        StyleBox horizontalStyleBox = _topCoords.GetThemeStylebox("normal");

        if (horizontalStyleBox is not null)
        {
            Vector2 charSize = _labelSettings.Font.GetCharSize(_topCoords.Text[0], _gridSettings.FontSize);

            float margin = _gridSettings.Margin.X - charSize.X / 2;

            horizontalStyleBox.ContentMarginLeft = margin;
            horizontalStyleBox.ContentMarginRight = margin;
        }
    }

    private void SetVerticalCoordsMargins()
    {
        StyleBox verticalStyleBox = _leftCoords.GetThemeStylebox("normal");

        if (verticalStyleBox is not null)
        {
            Vector2 charSize = _labelSettings.Font.GetCharSize(_leftCoords.Text[0], _gridSettings.FontSize);

            float margin = _gridSettings.Margin.Y + _topCoords.Size.Y - charSize.Y / 2;

            verticalStyleBox.ContentMarginTop = margin;
            verticalStyleBox.ContentMarginBottom = margin;
        }
    }

    private void SetHorizontalCoords()
    {
        string text = string.Join(" ", AlphaCoordinates[.._gridSettings.GridSize].ToCharArray());

        SetCoords(_topCoords, CoordinatePlacement.Top, text);
        SetCoords(_bottomCoords, CoordinatePlacement.Bottom, text);
    }

    private void SetVerticalCoords()
    {
        string text = string.Join("\n", Enumerable.Range(1, _gridSettings.GridSize).Reverse());

        SetCoords(_leftCoords, CoordinatePlacement.Left, text);
        SetCoords(_rightCoords, CoordinatePlacement.Right, text);
    }

    private void UpdateGrid()
    {
        _gridSettings.GridDimensions = _playArea.Size;
    }

    private void UpdatePlayArea()
    {
        _playArea.AddThemeConstantOverride("margin_top", _gridSettings.Margin.Y);
        _playArea.AddThemeConstantOverride("margin_bottom", _gridSettings.Margin.Y);

        _playArea.AddThemeConstantOverride("margin_left", _gridSettings.Margin.X);
        _playArea.AddThemeConstantOverride("margin_right", _gridSettings.Margin.X);
    }

    private void SetCoords(Label coordinate, CoordinatePlacement placement, string text)
    {
        coordinate.Visible = _gridSettings.CoordinatePlacement.HasFlag(placement);

        if (!coordinate.Visible)
        {
            return;
        }

        coordinate.Text = text;
    }
}

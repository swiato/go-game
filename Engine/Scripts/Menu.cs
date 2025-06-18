using Godot;
using System;

namespace Engine.Scripts;

public partial class Menu : Control
{
    [Export] private SpinBox _gridSize;
    [Export] private OptionButton _gameType;
    [Export] private OptionButton _blackPlayer;
    [Export] private OptionButton _whitePlayer;
    [Export] private Button _start;

    public override void _Ready()
    {
        InitializeControls();
        _start.Pressed += OnStartPressed;
    }

    public override void _ExitTree()
    {
        _start.Pressed -= OnStartPressed;
    }

    private void InitializeControls()
    {
        _gameType.Clear();

        foreach (GameType value in Enum.GetValues<GameType>())
        {
            _gameType.AddItem(value.ToString(), (int)value);
        }

        _blackPlayer.Clear();
        _whitePlayer.Clear();

        foreach (PlayerType value in Enum.GetValues<PlayerType>())
        {
            _blackPlayer.AddItem(value.ToString(), (int)value);
            _whitePlayer.AddItem(value.ToString(), (int)value);
        }
    }

    private void OnStartPressed()
    {
        GameStartedEventArgs eventArgs = new(
            (int)_gridSize.Value,
            (GameType)_gameType.GetSelectedId(),
            (PlayerType)_blackPlayer.GetSelectedId(),
            (PlayerType)_whitePlayer.GetSelectedId()
            );

        GameEvents.OnGameStarted(this, eventArgs);
    }
}

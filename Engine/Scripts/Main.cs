using Godot;
using Engine.Scenes.Dialogue;

namespace Engine.Scripts;

public partial class Main : Control
{
    [Export]
    private Control _game;

    [Export]
    private Control _menu;

    [Export]
    private Command[] _commands;

    public override void _Ready()
    {
        ShowGame(false);
        GameEvents.GameStarted += OnGameStarted;
    }

    public override void _ExitTree()
    {
        GameEvents.GameStarted -= OnGameStarted;
    }

    private void OnGameStarted(object sender, GameStartedEventArgs e)
    {
        ShowGame(true);

        foreach (Command command in _commands)
        {
            command.Execute();
        }
    }

    private void ShowGame(bool visible)
    {
        _game.Visible = visible;
        _menu.Visible = !visible;
    }
}

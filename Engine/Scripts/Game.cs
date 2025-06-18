using Godot;

namespace Engine.Scripts;

public partial class Game : Control
{
    public override void _Ready()
    {
        GameEvents.GameOver += OnGameOver;
    }

    private void OnGameOver(object sender, GameOverEventArgs e)
    {
        GameEvents.OnUserNotificationRequired(this, new(e.Result, NotificationType.Success));
        GetTree().Paused = true;
    }
}

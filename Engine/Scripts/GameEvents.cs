using System;

namespace Engine.Scripts;

public class GameEvents
{
    public static event EventHandler<UserNotificationEventArgs> UserNotificationRequired;
    public static event EventHandler<GameStartedEventArgs> GameStarted;
    public static event EventHandler<PlayedEventArgs> Played;
    public static event EventHandler<GameOverEventArgs> GameOver;

    public static void OnUserNotificationRequired(object sender, UserNotificationEventArgs e)
    {
        UserNotificationRequired?.Invoke(sender, e);
    }

    public static void OnGameStarted(object sender, GameStartedEventArgs e)
    {
        GameStarted?.Invoke(sender, e);
    }

    public static void OnPlayed(object sender, PlayedEventArgs e)
    {
        Played?.Invoke(sender, e);
    }

    public static void OnGameOver(object sender, GameOverEventArgs e)
    {
        GameOver?.Invoke(sender, e);
    }
}
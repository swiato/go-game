using System;

namespace Engine.Scripts;

public class UserNotificationEventArgs(string message, NotificationType type = NotificationType.Info) : EventArgs
{
    public string Message { get; } = message;
    public NotificationType Type { get; } = type;
}
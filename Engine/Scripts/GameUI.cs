using System;
using Godot;
using Domain.Go;

namespace Engine.Scripts;

public partial class GameUI : Control
{
    [Export]
    private Control _notification;

    [Export]
    private Button _pass;

    [Export]
    private Button _resign;

    [Export]
    private Timer _notificationTimer;

    public override void _Ready()
    {
        ToggleNotification(false);
        GameEvents.UserNotificationRequired += OnUserNotificationRequired;
        // _notification.GuiInput += OnNotificationGuiInput;
        _pass.Pressed += OnPassPressed;
        _resign.Pressed += OnResignPressed;
        _notificationTimer.Timeout += OnNotificationTimerTimeout;
    }

    public override void _ExitTree()
    {
        GameEvents.UserNotificationRequired -= OnUserNotificationRequired;
        // _notification.GuiInput -= OnNotificationGuiInput;
        _pass.Pressed -= OnPassPressed;
        _resign.Pressed -= OnResignPressed;
        _notificationTimer.Timeout -= OnNotificationTimerTimeout;
    }

    private void OnUserNotificationRequired(object sender, UserNotificationEventArgs e)
    {
        ToggleNotification(true, e.Message, e.Type);
    }

    // private void OnNotificationGuiInput(InputEvent @event)
    // {
    //     if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
    //     {
    //         ToggleNotification(false);
    //     }
    // }

    private void ToggleNotification(bool show, string text = "", NotificationType notificationType = NotificationType.Info)
    {
        Label label = _notification.GetNode<Label>("Label");
        label.Text = text;
        label.Modulate = GetNotificationColor(notificationType);
        _notification.Visible = show;

        if (show)
        {
            _notificationTimer.Start();
        }
    }

    private void OnNotificationTimerTimeout()
    {
        ToggleNotification(false);
    }


    private void OnPassPressed()
    {
        GameEvents.OnPlayed(this, new(Move.Pass()));
    }

    private void OnResignPressed()
    {
        GameEvents.OnPlayed(this, new(Move.Resign()));
    }

    private static Color GetNotificationColor(NotificationType notificationType)
    {
        switch (notificationType)
        {
            case NotificationType.Info:
                return Colors.White;
            case NotificationType.Warning:
                return Colors.Yellow;
            case NotificationType.Error:
                return Colors.Red;
            case NotificationType.Success:
                return Colors.Green;
            default:
                throw new NotImplementedException();
        }
    }
}
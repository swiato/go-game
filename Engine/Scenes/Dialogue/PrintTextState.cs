using Godot;

namespace Engine.Scenes.Dialogue;

public partial class PrintTextState : DialogueState
{
    [Export]
    private AudioStream[] _typingSounds;

    [Export]
    private AudioStreamPlayer _audioStreamPlayer;

    [Export]
    private Timer TypingTimer;

    public override void EnterState()
    {
        Dialogue.NameLabel.Text = Dialogue.CurrentNode.Name;
        Dialogue.TextLabel.Text = Dialogue.CurrentNode.Text;
        Dialogue.TextLabel.VisibleCharacters = 0;
        TypingTimer.Timeout += OnTypingTimerTimeout;
        Dialogue.GuiInput += OnDialogueGuiInput;
        TypingTimer.Start();
    }

    public override void ExitState()
    {
        Dialogue.TextLabel.VisibleCharacters = -1;
        _audioStreamPlayer.Stop();
        TypingTimer.Stop();
        Dialogue.GuiInput -= OnDialogueGuiInput;
        TypingTimer.Timeout -= OnTypingTimerTimeout;
    }

    private void OnDialogueGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
        {
            Dialogue.StateMachine.ChangeState<WaitForInputState>();
        }
    }

    private void OnTypingTimerTimeout()
    {
        if (Dialogue.TextLabel.VisibleCharacters < Dialogue.CurrentNode.Text.Length)
        {
            TypeCharacter();
            return;
        }

        Dialogue.StateMachine.ChangeState<WaitForInputState>();
    }

    private void TypeCharacter()
    {
        Dialogue.TextLabel.VisibleCharacters++;
        _audioStreamPlayer.Stream = _typingSounds[GD.RandRange(0, _typingSounds.Length - 1)];
        // _audioStreamPlayer.PitchScale = (float)GD.RandRange(0.95, 1.05);
        _audioStreamPlayer.Play();
        TypingTimer.Start();
    }
}
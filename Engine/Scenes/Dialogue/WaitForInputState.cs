using Godot;

namespace Engine.Scenes.Dialogue;

public partial class WaitForInputState : DialogueState
{
    [Export]
    private Label _nextLabel;

    public override void EnterState()
    {
        _nextLabel.Show();
        Dialogue.GuiInput += OnDialogueGuiInput;
        Dialogue.AnimationPlayer.Play("NextPulse");
    }

    public override void ExitState()
    {
        _nextLabel.Hide();
        Dialogue.GuiInput -= OnDialogueGuiInput;
        Dialogue.AnimationPlayer.Stop();
    }

    private void OnDialogueGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
        {
            Dialogue.StateMachine.ChangeState<NextDialogueState>();
        }
    }
}
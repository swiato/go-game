using Godot;

namespace Engine.Scenes.Dialogue;

public partial class HideDialogueState : DialogueState
{
    public override void EnterState()
    {
        Dialogue.NameLabel.Text = string.Empty;
        Dialogue.TextLabel.Text = string.Empty;
        Dialogue.AnimationPlayer.AnimationFinished += OnAnimationFinished;
        Dialogue.AnimationPlayer.Play("DialogueFadeOut");
    }

    public override void ExitState()
    {
        Dialogue.AnimationPlayer.AnimationFinished -= OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        Dialogue.StateMachine.ChangeState<HiddenState>();
    }
}
using Godot;

namespace Engine.Scenes.Dialogue;

public partial class ShowOptionsState : DialogueState
{
    [Export]
    private Control _choices;

    [Export]
    private PackedScene _choiceButton;

    public override void EnterState()
    {
        foreach (DialogueNode dialogue in Dialogue.CurrentNode.Children)
        {
            Button choiceButton = _choiceButton.Instantiate<Button>();
            choiceButton.Text = dialogue.Text;
            choiceButton.Pressed += () => OnChoiceButtonPressed(dialogue);

            _choices.AddChild(choiceButton);
        }
    }

    public override void ExitState()
    {
        foreach (Node choice in _choices.GetChildren())
        {
            choice.QueueFree();
        }
    }

    private void OnChoiceButtonPressed(DialogueNode dialogueNode)
    {
        Dialogue.SetCurrentNode(dialogueNode);
        Dialogue.StateMachine.ChangeState<NextDialogueState>();
    }
}
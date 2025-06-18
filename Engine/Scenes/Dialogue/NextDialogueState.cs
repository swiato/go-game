using System.Linq;
using Godot;

namespace Engine.Scenes.Dialogue;

public partial class NextDialogueState : DialogueState
{
    public override void EnterState()
    {
        NextDialogue(Dialogue.CurrentNode);
    }

    public override void ExitState()
    {
    }

    private void NextDialogue(DialogueNode dialogueNode)
    {
        if (dialogueNode.Children.Length == 0)
        {
            Dialogue.StateMachine.ChangeState<HideDialogueState>();
            return;
        }

        if (dialogueNode.Children.Where(child => child.Name == "Player").Any())
        {
            Dialogue.StateMachine.ChangeState<ShowOptionsState>();
            return;
        }

        int randomIndex = GD.RandRange(0, dialogueNode.Children.Length - 1);
        DialogueNode newDialogue = dialogueNode.Children[randomIndex];
        Dialogue.SetCurrentNode(newDialogue);
        Dialogue.StateMachine.ChangeState<PrintTextState>();
    }
}
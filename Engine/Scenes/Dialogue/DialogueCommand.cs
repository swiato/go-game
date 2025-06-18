using Godot;

namespace Engine.Scenes.Dialogue;

[GlobalClass]
public partial class DialogueCommand : Command
{
    [Export] private DialogueNode _dialogue;

    public override void Execute()
    {
        DialogueManager.Instance.StartDialogue(_dialogue);
    }
}
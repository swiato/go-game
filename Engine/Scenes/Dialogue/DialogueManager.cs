using Godot;

namespace Engine.Scenes.Dialogue;

// TODO: add string const class to hold animation names and other
public partial class DialogueManager : Control
{
    [Export]
    public Label NameLabel { get; private set; }

    [Export]
    public RichTextLabel TextLabel { get; private set; }

    [Export]
    public AnimationPlayer AnimationPlayer { get; set; }

    [Export]
    public StateMachine StateMachine { get; private set; }

    public DialogueNode CurrentNode { get; private set; }

    public static DialogueManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public void StartDialogue(DialogueNode dialogueNode)
    {
        GD.Print("StartDialogue");
        SetCurrentNode(dialogueNode);
        StateMachine.ChangeState<ShowDialogueState>();
    }

    public void SetCurrentNode(DialogueNode dialogueNode)
    {
        // change CurrentNode before calling ExitActions 
        // to prevent circular dependency using DialogueCommand
        DialogueNode previousNode = CurrentNode;
        CurrentNode = dialogueNode;

        TriggerActions(previousNode?.ExitActions);
        TriggerActions(CurrentNode?.EnterActions);
    }

    public void TriggerActions(Command[] commands)
    {
        if (commands is null)
        {
            return;
        }

        foreach (Command command in commands)
        {
            command.Execute();
        }
    }
}
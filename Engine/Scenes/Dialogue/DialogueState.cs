using Godot;

namespace Engine.Scenes.Dialogue;

public abstract partial class State : Node
{
    public abstract void EnterState();

    public abstract void ExitState();
}

public abstract partial class DialogueState : State
{
    protected DialogueManager Dialogue;

    public override void _Ready()
    {
        Dialogue = GetOwner<DialogueManager>();
    }
}
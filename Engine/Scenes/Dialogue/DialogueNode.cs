using Godot;

namespace Engine.Scenes.Dialogue;

[GlobalClass]
public partial class DialogueNode : Resource
{
    [Export]
    public string Name { get; set; }

    [Export]
    public string Text { get; set; }

    [Export]
    public DialogueNode[] Children { get; set; }

    [Export]
    public Command[] EnterActions { get; set; }

    [Export]
    public Command[] ExitActions { get; set; }
}
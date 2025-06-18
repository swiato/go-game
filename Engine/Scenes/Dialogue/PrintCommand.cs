using Godot;

namespace Engine.Scenes.Dialogue;

[GlobalClass]
public partial class PrintCommand : Command
{
    public override void Execute()
    {   
        GD.Print("PrintCommand.Execute");
    }
}
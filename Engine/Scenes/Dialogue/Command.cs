using Godot;

namespace Engine.Scenes.Dialogue;

[GlobalClass]
public abstract partial class Command : Resource
{
    public abstract void Execute();
}
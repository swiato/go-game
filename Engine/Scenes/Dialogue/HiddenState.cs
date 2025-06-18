namespace Engine.Scenes.Dialogue;

public partial class HiddenState : DialogueState
{
    public override void EnterState()
    {
        Dialogue.ProcessMode = ProcessModeEnum.Inherit;
        Dialogue.GetTree().Paused = false;
        Dialogue.Hide();
        Dialogue.SetCurrentNode(null);
    }

    public override void ExitState()
    {        
        Dialogue.ProcessMode = ProcessModeEnum.Always;
        Dialogue.GetTree().Paused = true;
        Dialogue.Show();
    }
}

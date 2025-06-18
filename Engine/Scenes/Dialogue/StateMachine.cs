using Godot;

using System.Linq;

namespace Engine.Scenes.Dialogue;

public partial class StateMachine : Node
{
    [Export]
    private State _currentState;
    
    private State[] _states;

    public override void _Ready()
    {
        _states = [.. GetChildren().OfType<State>()];
        _currentState.EnterState();
    }

    public void ChangeState<T>()
    {
        State newState = _states.First(state => state is T);

        if (_currentState == newState)
        {
            GD.Print("Attempt to change state for the same state!");
            // return;
        }

        _currentState.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }
}
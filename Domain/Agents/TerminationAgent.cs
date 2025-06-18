using Domain.Go;

namespace Domain.Agents;

public class TerminationAgent : IAgent
{
    private bool _disposed = false;
    private readonly IAgent _agent;
    private readonly ITerminationStrategy _terminationStrategy;

    public TerminationAgent(IAgent agent, ITerminationStrategy terminationStrategy)
    {
        _agent = agent;
        _terminationStrategy = terminationStrategy;
    }

    public Move SelectMove(IGameState gameState)
    {
        if (_terminationStrategy.ShouldPass(gameState))
        {
            return Move.Pass();
        }

        if (_terminationStrategy.ShouldResign(gameState))
        {
            return Move.Resign();
        }

        return _agent.SelectMove(gameState);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _agent.Dispose();
        }
        
        _disposed = true;
    }
}
using Domain.Go;

namespace Domain.Agents;

public interface IAgent : IDisposable
{
    Move SelectMove(IGameState gameState);
}

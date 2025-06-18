using Domain.Go;

namespace Domain.Agents;

public interface ITerminationStrategy
{
    bool ShouldPass(IGameState gameState);
    bool ShouldResign(IGameState gameState);
}

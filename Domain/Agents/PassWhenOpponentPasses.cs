using Domain.Go;

namespace Domain.Agents;

public class PassWhenOpponentPasses : ITerminationStrategy
{
    public bool ShouldPass(IGameState gameState)
    {
        if (gameState.LastMove is null)
        {
            return false;
        }

        return gameState.LastMove.IsPass;
    }

    public bool ShouldResign(IGameState gameState)
    {
        return false;
    }
}


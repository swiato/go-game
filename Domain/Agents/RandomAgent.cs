using Domain.Go;

namespace Domain.Agents;

public class RandomAgent : IAgent
{
    public Move SelectMove(IGameState gameState)
    {
        Move[] validMoves = [.. gameState.GetValidMoves()];
        int randomIndex = Random.Shared.Next(validMoves.Length);
        return validMoves[randomIndex];
    }

    public void Dispose()
    {
        
    }
}

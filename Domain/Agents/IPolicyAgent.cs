using Domain.Go;

namespace Domain.Agents;

public interface IPolicyAgent : IAgent
{
    IEnumerable<MovePrediction> PredictMoves(IGameState gameState);
}

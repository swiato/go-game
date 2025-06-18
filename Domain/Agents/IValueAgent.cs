using Domain.Encoders;
using Domain.Go;

namespace Domain.Agents;

public interface IValueAgent : IAgent
{
    IEncoder Encoder { get; }
    float Predict(IGameState gameState);
}

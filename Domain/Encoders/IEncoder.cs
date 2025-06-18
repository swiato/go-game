using Domain.Go;

namespace Domain.Encoders;

public interface IEncoder
{
    int[] Shape { get; }
    int[,,] Encode(IGameState gameState);
    int EncodePoint(Point point);
    Point DecodePointIndex(int index);
    int EncodeWinner(IGameState gameState, Player winner);
}

using Domain.Go;

namespace Domain.Encoders;

public class BetaGoEncoder(int boardSize) : IEncoder
{
    private readonly int _boardSize = boardSize;
    private const int _planesCount = 7;
    private readonly int[] _shape = [_planesCount, boardSize, boardSize];

    public int[] Shape => _shape;

    public int[,,] Encode(IGameState gameState)
    {
        int[,,] boardTensor = new int[_planesCount, _boardSize, _boardSize];

        Dictionary<Player, int> basePlane = new()
        {
            { gameState.NextPlayer, 0 },
            { gameState.NextPlayer.Other(), 3 }
        };

        foreach (Point point in gameState.Board.GetIntersections())
        {
            Chain? chain = gameState.Board.GetChain(point);

            if (chain is null)
            {
                if (gameState.DoesViolateKo(point))
                {
                    boardTensor[6, point.Row, point.Column] = 1;
                }
            }
            else
            {
                var libertyPlane = Math.Min(3, chain.Liberties.Count) - 1;
                libertyPlane += basePlane[chain.Player];
                boardTensor[libertyPlane, point.Row, point.Column] = 1;
            }
        }

        return boardTensor;
    }

    public int EncodePoint(Point point)
    {
        return _boardSize * point.Row + point.Column;
    }

    public Point DecodePointIndex(int index)
    {
        var row = index / _boardSize;
        var column = index % _boardSize;

        return new Point(row, column);
    }

    public int EncodeWinner(IGameState gameState, Player winner)
    {
        throw new NotImplementedException();
    }
}

using Domain.Go;

namespace Domain.Encoders;

public class SevenPlaneEncoder(int boardSize) : IEncoder
{
    private readonly int _boardSize = boardSize;
    private const int _planesCount = 7;
    private readonly int[] _shape = [boardSize, boardSize, _planesCount];

    public int[] Shape => _shape;

    public int[,,] Encode(IGameState gameState)
    {
        int[,,] boardTensor = new int[_boardSize, _boardSize, _planesCount];

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
                    boardTensor[point.Row, point.Column, 6] = 1;
                }
            }
            else
            {
                var libertyPlane = Math.Min(3, chain.Liberties.Count) - 1;
                libertyPlane += basePlane[chain.Player];
                boardTensor[point.Row, point.Column, libertyPlane] = 1;
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
        if (winner == gameState.NextPlayer)
        {
            return 1;
        }

        if (winner == gameState.NextPlayer.Other())
        {
            return -1;
        }

        return 0;
    }
}

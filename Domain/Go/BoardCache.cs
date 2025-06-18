namespace Domain.Go;

internal class BoardCache
{
    private readonly int _boardSize;
    private readonly Dictionary<Point, Intersection> _intersections = [];
    private readonly Dictionary<(Player, Point), long> _zobristHashCodes = [];

    public BoardCache(int boardSize)
    {
        _boardSize = boardSize;

        InitializeBoard();
    }

    public Point[] GetIntersections()
    {
        return [.._intersections.Keys];
    }

    public Point[] GetNeighbors(Point point)
    {
        if (_intersections.TryGetValue(point, out var intersection))
        {
            return intersection.Neighbors;
        }

        return [];
    }

    public Point[] GetCorners(Point point)
    {
        if (_intersections.TryGetValue(point, out var intersection))
        {
            return intersection.Corners;
        }

        return [];
    }

    public long GetZobristHash(Player player, Point point)
    {
        return _zobristHashCodes[(player, point)];
    }

    private void InitializeBoard()
    {
        Point[,] _points = new Point[_boardSize, _boardSize];

        for (var row = 0; row < _boardSize; row++)
        {
            for (var column = 0; column < _boardSize; column++)
            {
                _points[row, column] = new(row, column);
            }
        }

        for (var row = 0; row < _boardSize; row++)
        {
            for (var column = 0; column < _boardSize; column++)
            {
                Point point = _points[row, column];

                _intersections[point] = new Intersection([.. GetNeighbours(row, column, _points)], [.. GetCorners(row, column, _points)]);
                _zobristHashCodes[(Player.Black, point)] = Random.Shared.NextInt64();
                _zobristHashCodes[(Player.White, point)] = Random.Shared.NextInt64();
            }
        }
    }

    private IEnumerable<Point> GetNeighbours(int row, int column, Point[,] points)
    {
        /*
            . . .
            n x .
            . . .
        */
        if (column > 0)
        {
            yield return points[row, column - 1];
        }

        /*
            . . .
            . x n
            . . .
        */
        if (column + 1 < _boardSize)
        {
            yield return points[row, column + 1];
        }

        /*
            . n .
            . x .
            . . .
        */
        if (row > 0)
        {
            yield return points[row - 1, column];
        }

        /*
            . . .
            . x .
            . n .
        */
        if (row + 1 < _boardSize)
        {
            yield return points[row + 1, column];
        }
    }

    private IEnumerable<Point> GetCorners(int row, int column, Point[,] points)
    {
        /*
            c . .
            . x .
            . . .
        */
        if (column > 0 && row > 0)
        {
            yield return points[row - 1, column - 1];
        }

        /*
            . . c
            . x .
            . . .
        */
        if (column + 1 < _boardSize && row > 0)
        {
            yield return points[row - 1, column + 1];
        }

        /*
            . . .
            . x .
            . . c
        */
        if (column + 1 < _boardSize && row + 1 < _boardSize)
        {
            yield return points[row + 1, column + 1];
        }

        /*
            . . .
            . x .
            c . .
        */
        if (column > 0 && row + 1 < _boardSize)
        {
            yield return points[row + 1, column - 1];
        }
    }
}

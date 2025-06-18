namespace Domain.Go;

public static class BoardExtensions
{
    private static readonly Dictionary<int, BoardCache> _cachedBoards = [];

    public static Point[] GetIntersections(this Board board)
    {
        return GetBoardCache(board).GetIntersections();
    }

    public static Point[] GetNeighbors(this Board board, Point point)
    {
        return GetBoardCache(board).GetNeighbors(point);
    }

    public static Point[] GetCorners(this Board board, Point point)
    {
        return GetBoardCache(board).GetCorners(point);
    }

    public static long GetZobristHash(this Board board, Player player, Point point)
    {
        return GetBoardCache(board).GetZobristHash(player, point);
    }

    private static BoardCache GetBoardCache(Board board)
    {
        if (_cachedBoards.TryGetValue(board.Size, out var boardCache))
        {
            return boardCache;
        }

        return _cachedBoards[board.Size] = new BoardCache(board.Size);
    }
}

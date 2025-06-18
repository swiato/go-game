using System.Text;
using Domain.Common;

namespace Domain.Go;

public class Board(int size) : IEquatable<Board>
{
    private readonly Dictionary<Point, Chain> _stones = [];
    public Dictionary<Player, int> _capturedStones = new() { { Player.Black, 0 }, { Player.White, 0 } };
    private long _zobristHash = 0L;
    public int Size => size;
    public long Hash => _zobristHash;
    public bool IsEmpty => _stones.Count == 0;
    public IEnumerable<Point> Stones => _stones.Keys;
    public IEnumerable<Chain> Chains => _stones.Values;

    public Board(Board board) : this(board.Size)
    {
        _stones = board._stones.ToDictionary();
        _capturedStones = board._capturedStones.ToDictionary();
        _zobristHash = board._zobristHash;
    }

    public void PlaceStone(Player player, Point point)
    {
        ValidatePlacePoint(point);

        ApplyZobristHash(player, point);

        var (playerChains, opponentChains) = GetNeighboringChains(player, point);

        UpdatePlayerChains(playerChains);

        UpdateOpponentChains(opponentChains, point);

        RemoveIfChainDead(point);
    }

    public int GetCapturedStones(Player player)
    {
        return _capturedStones[player];
    }

    public bool IsOutsideGrid(Point point)
    {
        return !IsOnGrid(point);
    }

    public bool IsOnGrid(Point point)
    {
        return IsInRange(point.Row) && IsInRange(point.Column);
    }

    public bool IsPointEmpty(Point point)
    {
        return !IsPointTaken(point);
    }

    public bool IsPointTaken(Point point)
    {
        return _stones.ContainsKey(point);
    }

    /*
        . . .   . . .   . x x
        x x .   x x x   x e x
        e x .   x e x   x x x
    */
    public bool IsPointAnEye(Player player, Point point)
    {
        if (IsPointTaken(point))
        {
            return false;
        }

        if (AnyOpponentNeighbors(player, point))
        {
            return false;
        }

        return MostlyFriendlyCorners(player, point);
    }

    public Player GetPlayer(Point point)
    {
        return GetChain(point)?.Player ?? Player.None;
    }

    public Chain? GetChain(Point point)
    {
        _stones.TryGetValue(point, out var chain);
        return chain;
    }

    public string PrintBoard()
    {
        var stringBuilder = new StringBuilder();

        for (var row = Size; row > 0; row--)
        {
            var bump = row > 9 ? string.Empty : " ";

            stringBuilder.Append(bump);
            stringBuilder.Append(row);

            for (var column = 0; column < Size; column++)
            {
                var player = GetPlayer(new Point(Size - row, column));
                stringBuilder.Append($" {player.Symbol()} ");
            }

            stringBuilder.AppendLine();
        }

        stringBuilder.Append("   ");
        stringBuilder.AppendJoin("  ", BoardHelper.ColumnNames[..Size].ToCharArray());

        return stringBuilder.ToString();
    }

    public bool Equals(Board? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _zobristHash == other._zobristHash;

        // return _grid.Keys.SequenceEqual(other._grid.Keys);
    }

    public override bool Equals(object? obj) => base.Equals(obj as Board);

    public override int GetHashCode()
    {
        return _zobristHash.GetHashCode();
        // return _grid.Keys.GetCombinedHashCode();
    }

    public static bool operator ==(Board first, Board second)
    {
        if (first is null)
        {
            if (second is null)
            {
                return true;
            }

            return false;
        }

        return first.Equals(second);
    }

    public static bool operator !=(Board first, Board second) => !(first == second);

    public override string ToString()
    {
        return PrintBoard();
    }

    private void ValidatePlacePoint(Point point)
    {
        if (IsOutsideGrid(point))
        {
            throw new ArgumentException(ErrorMessages.PointIsOutsideGrid);
        }

        if (IsPointTaken(point))
        {
            throw new ArgumentException(ErrorMessages.PointIsAlreadyTaken);
        }
    }

    private void ApplyZobristHash(Player player, Point point)
    {
        _zobristHash ^= this.GetZobristHash(player, point);
    }

    private (IEnumerable<Chain> playerChains, IEnumerable<Chain> opponentChains) GetNeighboringChains(Player player, Point point)
    {
        HashSet<Chain> playerChains = [];
        HashSet<Chain> opponentChains = [];
        List<Point> liberties = [];

        foreach (Point neighbor in this.GetNeighbors(point))
        {
            Chain? neighborChain = GetChain(neighbor);

            if (neighborChain is null)
            {
                liberties.Add(neighbor);
            }
            else if (neighborChain.Player == player)
            {
                playerChains.Add(neighborChain);
            }
            else
            {
                opponentChains.Add(neighborChain);
            }
        }

        Chain newChain = new(player, [point], [.. liberties]);

        playerChains.Add(newChain);

        return (playerChains, opponentChains);
    }

    private void UpdatePlayerChains(IEnumerable<Chain> playerChains)
    {
        Chain combinedChain = CombineChains(playerChains);
        UpdateBoard(combinedChain);
    }

    private void UpdateOpponentChains(IEnumerable<Chain> opponentChains, Point point)
    {
        foreach (Chain opponentChain in opponentChains)
        {
            Chain updatedChain = opponentChain.RemoveLiberty(point);

            if (updatedChain.Liberties.Count == 0)
            {
                RemoveChain(opponentChain);
            }
            else
            {
                UpdateBoard(updatedChain);
            }
        }
    }

    private void RemoveIfChainDead(Point point)
    {
        Chain playerChain = GetChain(point)!;

        if (playerChain.Liberties.Count == 0)
        {
            RemoveChain(playerChain);
        }
    }

    private static Chain CombineChains(IEnumerable<Chain> chains)
    {
        return chains.Aggregate((current, next) => current.Merge(next));
    }

    private void RemoveChain(Chain chain)
    {
        AddCapturedStones(chain.Player.Other(), chain.Stones.Count);

        foreach (Point stone in chain.Stones)
        {
            RemoveStone(chain.Player, stone);
        }
    }

    private void RemoveStone(Player player, Point stone)
    {
        foreach (Point neighbor in this.GetNeighbors(stone))
        {
            ReturnLibertyToOpponent(player, neighbor, stone);
        }

        _stones.Remove(stone);
        ApplyZobristHash(player, stone);
    }

    private void ReturnLibertyToOpponent(Player player, Point point, Point liberty)
    {
        Chain? chain = GetChain(point);

        if (chain is null || chain.Player == player)
        {
            return;
        }

        chain = chain.AddLiberty(liberty);
        UpdateBoard(chain);
    }

    private void UpdateBoard(Chain chain)
    {
        foreach (Point stone in chain.Stones)
        {
            _stones[stone] = chain;
        }
    }

    private void AddCapturedStones(Player player, int prisoners)
    {
        _capturedStones[player] += prisoners;
    }

    private bool AnyOpponentNeighbors(Player player, Point point)
    {
        foreach (Point neighbor in this.GetNeighbors(point))
        {
            var neighborPlayer = GetPlayer(neighbor);

            if (neighborPlayer != player)
            {
                return true;
            }
        }

        return false;
    }

    private bool MostlyFriendlyCorners(Player player, Point point)
    {
        int friendlyCorners = 0;

        Point[] corners = this.GetCorners(point);

        foreach (Point corner in corners)
        {
            Player cornerPlayer = GetPlayer(corner);

            if (cornerPlayer == player)
            {
                friendlyCorners += 1;
            }
        }

        return friendlyCorners == corners.Length || friendlyCorners >= 3;
    }

    private bool IsInRange(int value)
    {
        return 0 <= value && value < Size;
    }
}

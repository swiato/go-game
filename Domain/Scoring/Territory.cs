using Domain.Go;
using Domain.Extensions;

namespace Domain.Scoring;

public class Territory
{
    public int BlackTerritory { get; }
    public int WhiteTerritory { get; }
    public int BlackStones { get; }
    public int WhiteStones { get; }
    public int Dame { get; }
    public List<Point> DamePoints { get; } = [];

    public Territory(Dictionary<Point, string> territory)
    {
        foreach (var (point, status) in territory)
        {
            switch (status)
            {
                case nameof(Player.Black):
                    BlackStones++;
                    break;
                case nameof(Player.White):
                    WhiteStones++;
                    break;
                case "territory_b":
                    BlackTerritory++;
                    break;
                case "territory_w":
                    WhiteTerritory++;
                    break;
                case "dame":
                    Dame++;
                    DamePoints.Add(point);
                    break;
            }
        }
    }

    public static GameResult ComputeGameResult(IGameState gameState)
    {
        var territory = EvaluateTerritory(gameState.Board);

        return new GameResult(territory.BlackTerritory + territory.BlackStones, territory.WhiteTerritory + territory.WhiteStones, Komi: 7.5f);
    }

    public static Territory EvaluateTerritory(Board board)
    {
        var status = new Dictionary<Point, string>();

        foreach (var point in board.GetIntersections())
        {
            if (status.ContainsKey(point))
            {
                continue;
            }

            Player player = board.GetPlayer(point);

            if (player is not Player.None)
            {
                status[point] = player.ToString();
            }
            else
            {
                var (group, neighbors) = CollectRegion(point, board);
                string fillWith;

                if (neighbors.Count == 1)
                {
                    var neighborStone = neighbors.Pop();
                    var stoneString = neighborStone == Player.Black ? "b" : "w";

                    fillWith = "territory_" + stoneString;
                }
                else
                {
                    fillWith = "dame";
                }

                foreach (var stone in group)
                {
                    status[stone] = fillWith;
                }
            }
        }

        return new Territory(status);
    }

    private static (Point[], HashSet<Player>) CollectRegion(Point startPosition, Board board, Dictionary<Point, bool>? visited = null)
    {
        if (visited is null)
        {
            visited = [];
        }
        else if (visited.ContainsKey(startPosition))
        {
            return ([], []);
        }

        Point[] allPoints = [startPosition];
        HashSet<Player> allBorders = [];
        visited[startPosition] = true;
        Player player = board.GetPlayer(startPosition);

        foreach (Point nextPoint in board.GetNeighbors(startPosition))
        {
            var neighbor = board.GetPlayer(nextPoint);

            if (neighbor == player)
            {
                var (points, borders) = CollectRegion(nextPoint, board, visited);

                allPoints = [.. allPoints, .. points];
                allBorders = [.. allBorders, .. borders];
            }
            else
            {
                allBorders.Add(neighbor);
            }
        }

        return (allPoints, allBorders);
    }
}

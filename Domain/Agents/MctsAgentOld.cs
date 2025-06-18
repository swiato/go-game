using Domain.Extensions;
using Domain.Go;

namespace Domain.Agents;

public class MctsAgentOld(int rounds = 1000, float temperature = 1.4f, IAgent? rolloutAgent = null) : IAgent
{
    private readonly int _rounds = rounds;
    private readonly float _temperature = temperature;
    private readonly IAgent _agent = rolloutAgent ?? new RandomAgent();

    public Move SelectMove(IGameState gameState)
    {
        MctsNodeOld root = new(gameState);

        for (var i = 0; i < _rounds; i++)
        {
            MctsNodeOld node = root;

            while (node.IsGameOn() && node.NoRoomForAChild())
            {
                node = SelectChild(node);
            }

            if (node.HasRoomForAChild())
            {
                node = node.AddRandomChild();
            }

            Player winner = SimulateGame(node.GameState);

            while (node.Parent != null)
            {
                node.RecordWin(winner);
                node = node.Parent;
            }

            node.RecordWin(winner);
        }

        // var scoredMoves = root.Children
        //     .Select(child => (score: child.WinningFraction(gameState.NextPlayer), move: child.Move, rolloutsNumber: child.RolloutsNumber))
        //     .OrderByDescending(result => result.score)
        //     .Take(10);

        // foreach (var (score, move, rolloutsNumber) in scoredMoves)
        // {
        //     Console.WriteLine($"{move} - {score} ({rolloutsNumber})");
        // }

        Move bestMove = Move.Pass();
        float bestPercentage = -1f;

        foreach (MctsNodeOld child in root.Children)
        {
            float childPercentage = child.WinningFraction(gameState.NextPlayer);

            if (childPercentage > bestPercentage)
            {
                bestMove = child.Move!;
                bestPercentage = childPercentage;
            }
        }

        Console.WriteLine($"Select move {bestMove} with win percentage {bestPercentage}");

        return bestMove;
    }

    public void Dispose()
    {
        
    }

    private MctsNodeOld SelectChild(MctsNodeOld node)
    {
        int totalRollouts = node.Children.Sum(child => child.RolloutsNumber);
        float logRollouts = MathF.Log(totalRollouts);

        float bestScore = -1f;
        MctsNodeOld bestChild = node;

        foreach (MctsNodeOld child in node.Children)
        {
            float winPercentage = child.WinningFraction(node.GameState.NextPlayer);
            float explorationFactor = MathF.Sqrt(logRollouts / child.RolloutsNumber);
            float uctScore = winPercentage + _temperature * explorationFactor;

            if (uctScore > bestScore)
            {
                bestScore = uctScore;
                bestChild = child;
            }
        }

        return bestChild;
    }

    private Player SimulateGame(IGameState gameState)
    {
        while (!gameState.IsOver())
        {
            Move botMove = _agent.SelectMove(gameState);
            gameState = gameState.ApplyMove(botMove);
        }

        return gameState.GetWinner();
    }

    private class MctsNodeOld(IGameState gameState, MctsNodeOld? parent = null, Move? move = null)
    {
        private readonly List<Move> _unvisitedMoves = [.. gameState.GetValidMoves().DefaultIfEmpty(Move.Pass())];
        private readonly Dictionary<Player, int> _wins = new() { { Player.Black, 0 }, { Player.White, 0 } };
        private readonly List<MctsNodeOld> _children = [];
        private int _rolloutsNumber = 0;

        public IGameState GameState => gameState;
        public MctsNodeOld? Parent => parent;
        public Move? Move => move;
        public IEnumerable<MctsNodeOld> Children => _children;
        public int RolloutsNumber => _rolloutsNumber;

        public MctsNodeOld AddRandomChild()
        {
            int randomIndex = Random.Shared.Next(_unvisitedMoves.Count);
            Move randomMove = _unvisitedMoves.Pop(randomIndex);
            IGameState newState = GameState.ApplyMove(randomMove);
            MctsNodeOld newNode = new(newState, this, randomMove);
            _children.Add(newNode);

            return newNode;
        }

        public bool HasRoomForAChild()
        {
            return _unvisitedMoves.Count > 0;
        }

        public bool NoRoomForAChild()
        {
            return !HasRoomForAChild();
        }

        public bool IsGameOver()
        {
            return GameState.IsOver();
        }

        public bool IsGameOn()
        {
            return !IsGameOver();
        }

        public void RecordWin(Player player)
        {
            _wins[player]++;
            _rolloutsNumber++;
        }

        public float WinningFraction(Player player)
        {
            return (float)_wins[player] / _rolloutsNumber;
        }
    }
}

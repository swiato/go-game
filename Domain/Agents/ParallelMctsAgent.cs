using Domain.Common;
using Domain.Go;
using System.Collections.Concurrent;
using System.Text;

namespace Domain.Agents;

public class ParallelMctsAgent(int rounds = 10000, float temperature = 1f, IAgent? policyAgent = null) : IAgent
{
    private static readonly int Parallelism = Environment.ProcessorCount;
    private readonly int _rounds = rounds;
    private readonly float _temperature = temperature;
    private readonly IAgent _policyAgent = policyAgent ?? new RandomAgent();

    public Move SelectMove(IGameState gameState)
    {
        ParallelMctsNode root = new(gameState);

        int iterations = _rounds / Parallelism;

        Parallel.For(0, Parallelism, _ =>
        {
            for (int i = 0; i < iterations; i++)
            {
                ParallelMctsNode node = SelectNode(root);
                Player winner = SimulateGame(node.GameState);
                Backpropagate(node, winner);
            }
        });

        ParallelMctsNode bestChild = root.BestChild();
        return bestChild.Move!;
    }

    public ParallelMctsNode SelectNode(ParallelMctsNode node)
    {
        while (node.IsGameOn())
        {
            if (node.NotFullyExpanded())
            {
                return node.Expand();
            }

            node = node.SelectChild(_temperature);
        }

        return node;
    }

    public Player SimulateGame(IGameState gameState)
    {
        while (!gameState.IsOver())
        {
            Move move = _policyAgent.SelectMove(gameState);
            gameState = gameState.ApplyMove(move);
        }

        return gameState.GetWinner();
    }

    public void Backpropagate(ParallelMctsNode? node, Player winner)
    {
        while (node != null)
        {
            node.RecordWin(winner);
            node = node.Parent;
        }
    }

    public void Dispose()
    {

    }

    public class ParallelMctsNode(IGameState gameState, ParallelMctsNode? parent = null, Move? move = null)
    {
        private readonly ConcurrentQueue<Move> _unvisitedMoves = new(GetValidMoves(gameState));
        private readonly ConcurrentDictionary<Player, int> _wins = new([new(Player.Black, 0), new(Player.White, 0)]);
        private readonly ConcurrentBag<ParallelMctsNode> _children = [];
        private int _visits = 0;

        public IGameState GameState => gameState;
        public ParallelMctsNode? Parent => parent;
        public Move? Move => move;
        public ConcurrentBag<ParallelMctsNode> Children => _children;
        public int Visits => _visits;

        public ParallelMctsNode Expand()
        {
            if (_unvisitedMoves.TryDequeue(out var move))
            {
                IGameState nextState = GameState.ApplyMove(move);
                ParallelMctsNode child = new(nextState, this, move);
                _children.Add(child);

                return child;
            }

            throw new IndexOutOfRangeException(ErrorMessages.NodeFullyExpanded);
        }

        public ParallelMctsNode BestChild()
        {
            float bestScore = -1f;
            ParallelMctsNode? bestChild = null;

            foreach (ParallelMctsNode child in Children)
            {
                float winPercentage = child.WinRate(GameState.NextPlayer);

                if (winPercentage > bestScore)
                {
                    bestScore = winPercentage;
                    bestChild = child;
                }
            }

            return bestChild!;
        }

        public ParallelMctsNode SelectChild(float temperature)
        {
            float bestScore = -1f;
            ParallelMctsNode? bestChild = null;
            float logParentVisits = MathF.Log(Visits);

            foreach (ParallelMctsNode child in Children)
            {
                float winRate = child.WinRate(GameState.NextPlayer);
                float exploration = MathF.Sqrt(logParentVisits / child.Visits);
                float uctScore = winRate + temperature * exploration;

                if (float.IsNaN(uctScore))
                {
                    uctScore = 0f;
                }

                if (uctScore > bestScore)
                {
                    bestScore = uctScore;
                    bestChild = child;
                }
            }

            return bestChild!;
        }

        public bool NotFullyExpanded()
        {
            return _unvisitedMoves.Count > 0;
        }

        public bool IsGameOn()
        {
            return !GameState.IsOver();
        }

        public void RecordWin(Player player)
        {
            _wins.AddOrUpdate(player, 1, (key, oldValue) => oldValue + 1);
            Interlocked.Increment(ref _visits);
        }

        public int GetWins(Player player)
        {
            return _wins[player];
        }

        public float WinRate(Player player)
        {
            return (float)_wins[player] / _visits;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine(GameState.ToString());

            foreach (var (player, score) in _wins)
            {
                stringBuilder.AppendLine($"Player: {player}, Wins: {score}, Visits: {_visits}, Win%: {WinRate(player)}");
            }

            return stringBuilder.ToString();
        }

        private static Move[] GetValidMoves(IGameState gameState)
        {
            Move[] moves = [.. gameState.GetValidMoves()];
            Random.Shared.Shuffle(moves);

            return moves;
        }
    }
}

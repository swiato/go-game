using Domain.Common;
using Domain.Go;
using System.Text;

namespace Domain.Agents;

public class MctsAgent(int rounds = 500, float temperature = 1.4f, IAgent? rolloutAgent = null) : IAgent
{
    private readonly int _rounds = rounds;
    private readonly float _temperature = temperature;
    private readonly IAgent _rolloutAgent = rolloutAgent ?? new RandomAgent();

    public Move SelectMove(IGameState gameState)
    {
        MctsNode root = new(gameState);

        for (int round = 0; round < _rounds; round++)
        {
            MctsNode node = SelectNode(root);
            Player winner = SimulateGame(node.GameState);
            Backpropagate(node, winner);
        }

        MctsNode bestChild = root.BestChild();
        return bestChild.Move!;
    }

    public MctsNode SelectNode(MctsNode node)
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
            Move move = _rolloutAgent.SelectMove(gameState);
            gameState = gameState.ApplyMove(move);
        }

        return gameState.GetWinner();
    }

    public void Backpropagate(MctsNode? node, Player winner)
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

    public class MctsNode(IGameState gameState, MctsNode? parent = null, Move? move = null)
    {
        private readonly Queue<Move> _unvisitedMoves = new(GetValidMoves(gameState));
        private readonly Dictionary<Player, int> _wins = new() { { Player.Black, 0 }, { Player.White, 0 } };
        private readonly List<MctsNode> _children = [];
        private int _visits = 0;

        public IGameState GameState => gameState;
        public MctsNode? Parent => parent;
        public Move? Move => move;
        public IList<MctsNode> Children => _children;
        public int Visits => _visits;

        public MctsNode Expand()
        {
            if (_unvisitedMoves.Count == 0)
            {
                throw new IndexOutOfRangeException(ErrorMessages.NodeFullyExpanded);
            }

            Move move = _unvisitedMoves.Dequeue();
            IGameState nextState = GameState.ApplyMove(move);
            MctsNode child = new(nextState, this, move);
            _children.Add(child);

            return child;
        }

        public MctsNode BestChild()
        {
            float bestScore = -1f;
            MctsNode? bestChild = null;

            for (int i = 0; i < Children.Count; i++)
            {
                MctsNode child = Children[i];

                float winPercentage = child.WinRate(GameState.NextPlayer);

                if (winPercentage > bestScore)
                {
                    bestScore = winPercentage;
                    bestChild = child;
                }
            }

            return bestChild!;
        }

        public MctsNode SelectChild(float temperature)
        {
            float bestScore = float.MinValue;
            MctsNode? bestChild = null;
            float logParentVisits = MathF.Log(Visits);

            for (int i = 0; i < Children.Count; i++)
            {
                MctsNode child = Children[i];

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
            _wins[player]++;
            _visits++;
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

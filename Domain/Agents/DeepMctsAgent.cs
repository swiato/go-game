using Domain.Common;
using Domain.Go;

namespace Domain.Agents;

public class DeepMctsAgent(IPolicyAgent policyAgent, IValueAgent valueAgent, IAgent rolloutAgent, float lambda = 0.5f, int rounds = 1000, float cPuct = 5.0f) : IAgent
{
    private readonly int _rounds = rounds;
    private readonly float _cPuct = cPuct;
    private readonly IPolicyAgent _policyAgent = policyAgent;
    private readonly IValueAgent _valueAgent = valueAgent;
    private readonly IAgent _rolloutAgent = rolloutAgent;
    private readonly float _lambda = lambda;
    private bool _disposed = false;

    public Move SelectMove(IGameState gameState)
    {
        DeepMctsNode root = new(gameState, _policyAgent.PredictMoves(gameState));

        for (int round = 0; round < _rounds; round++)
        {
            DeepMctsNode node = SelectNode(root);
            float reward = CalculateReward(node.GameState);
            Backpropagate(node, reward);
        }

        DeepMctsNode bestChild = root.BestChild();
        return bestChild.Move!;
    }

    public DeepMctsNode SelectNode(DeepMctsNode node)
    {
        while (node.IsGameOn())
        {
            if (node.NotFullyExpanded())
            {
                return node.Expand(_policyAgent);
            }

            node = node.SelectChild(_cPuct);
        }

        return node;
    }

    public float CalculateReward(IGameState gameState)
    {
        float rollout = _lambda == 0f ? 0f : SimulateGame(gameState);
        float value = _lambda == 1f ? 0f : _valueAgent.Predict(gameState);

        float weightedValue = (1 - _lambda) * value + _lambda * rollout;
        return weightedValue;
    }

    public float SimulateGame(IGameState gameState)
    {
        Player currentPlayer = gameState.NextPlayer;

        while (!gameState.IsOver())
        {
            Move move = _rolloutAgent.SelectMove(gameState);
            gameState = gameState.ApplyMove(move);
        }

        Player winner = gameState.GetWinner();

        if (winner == Player.None)
        {
            return 0.5f;
        }

        return winner == currentPlayer ? 1 : 0;
    }

    public void Backpropagate(DeepMctsNode? node, float value)
    {
        while (node != null)
        {
            node.RecordWin(value);
            node = node.Parent;
            value = 1 - value;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _policyAgent?.Dispose();
            _valueAgent?.Dispose();
            _rolloutAgent?.Dispose();
        }

        _disposed = true;
    }

    public class DeepMctsNode(IGameState gameState, IEnumerable<MovePrediction> unvisitedMoves, DeepMctsNode? parent = null, MovePrediction? move = null)
    {
        private readonly Queue<MovePrediction> _unvisitedMoves = new(unvisitedMoves.DefaultIfEmpty(MovePrediction.Pass()));
        private readonly List<DeepMctsNode> _children = [];
        private float _value = 0f;
        private int _visits = 0;
        private float _priorProbability = move?.Probability ?? 0f;

        public IGameState GameState => gameState;
        public DeepMctsNode? Parent => parent;
        public MovePrediction? Move => move;
        public IList<DeepMctsNode> Children => _children;
        public int Visits => _visits;
        public float PriorProbability => _priorProbability;

        public DeepMctsNode Expand(IPolicyAgent policyAgent)
        {
            if (_unvisitedMoves.Count == 0)
            {
                throw new IndexOutOfRangeException(ErrorMessages.NodeFullyExpanded);
            }

            MovePrediction move = _unvisitedMoves.Dequeue();
            IGameState nextState = GameState.ApplyMove(move);
            IEnumerable<MovePrediction> unvisitedMoves = policyAgent.PredictMoves(nextState);

            DeepMctsNode child = new(nextState, unvisitedMoves, this, move);
            _children.Add(child);

            return child;
        }

        public DeepMctsNode BestChild()
        {
            float bestScore = float.MinValue;
            DeepMctsNode? bestChild = null;

            for (int i = 0; i < Children.Count; i++)
            {
                DeepMctsNode child = Children[i];

                float mostVisits = child.Visits;

                if (mostVisits > bestScore)
                {
                    bestScore = mostVisits;
                    bestChild = child;
                }
            }

            return bestChild!;
        }

        public DeepMctsNode SelectChild(float cPuct)
        {
            float bestScore = float.MinValue;
            DeepMctsNode? bestChild = null;

            for (int i = 0; i < Children.Count; i++)
            {
                DeepMctsNode child = Children[i];

                float qValue = child.WinRate();
                float uValue = cPuct * child.PriorProbability * (MathF.Sqrt(Visits) / (1 + child.Visits));

                if (float.IsNaN(uValue))
                {
                    Console.WriteLine($"U value is nan: {uValue}");
                    uValue = 0f;
                }

                float score = qValue + uValue;

                if (score > bestScore)
                {
                    bestScore = score;
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

        public void RecordWin(float value)
        {
            _value += value;
            _visits++;
        }

        public float WinRate()
        {
            return _value / _visits;
        }

        // public override string ToString()
        // {
        //     StringBuilder stringBuilder = new();

        //     stringBuilder.AppendLine(GameState.ToString());

        //     foreach (var (player, score) in _wins)
        //     {
        //         stringBuilder.AppendLine($"Player: {player}, Wins: {score}, Visits: {_visits}, Win%: {WinRate(player)}");
        //     }

        //     return stringBuilder.ToString();
        // }
    }
}

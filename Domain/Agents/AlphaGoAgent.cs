using Domain.Go;

namespace Domain.Agents;

public class AlphaGoAgent(IPolicyAgent policyAgent, IAgent fastPolicyAgent, IValueAgent valueAgent, float lambda = 1f, int rounds = 500, int depth = 10) : IAgent
{
    private readonly IPolicyAgent _policyAgent = policyAgent;
    private readonly IAgent _fastPolicyAgent = fastPolicyAgent;
    private readonly IValueAgent _valueAgent = valueAgent;
    private AlphaGoNode _root = new();
    private readonly float _lambda = lambda;
    private readonly int _rounds = rounds;
    private readonly int _depth = depth;
    private bool _disposed = false;

    public Move SelectMove(IGameState gameState)
    {
        for (int round = 0; round < _rounds; round++)
        {
            IGameState currentState = gameState;
            AlphaGoNode node = _root;

            for (int depth = 0; depth < _depth; depth++)
            {
                if (node.Children.Length == 0)
                {
                    if (currentState.IsOver())
                    {
                        break;
                    }

                    MovePrediction[] moves = PredictMoves(currentState);
                    node.ExpandChildren(moves);
                }

                node = node.SelectChild();

                currentState = currentState.ApplyMove(node.Move);
            }

            float value = _valueAgent?.Predict(currentState) ?? 0f;
            int rollout = PolicyRollout(currentState);

            // Console.WriteLine($"Round: {round}, Move: {node.Move.Point.ToA1Coordinates(19)}, policy agent: {rollout}, value agent: {value}");

            float weightedValue = (1 - _lambda) * value + _lambda * rollout;

            node.UpdateValues(weightedValue);
        }

        int visits = 0;
        AlphaGoNode? bestChild = null;

        for (int i = 0; i < _root.Children.Length; i++)
        {
            AlphaGoNode child = _root.Children[i];

            if (child.Visits > visits)
            {
                visits = child.Visits;
                bestChild = child;
            }
        }
        // AlphaGoNode bestChild = _root.Children.Aggregate((a, b) => a.Visits > b.Visits ? a : b);

        _root = new();

        return bestChild!.Move;
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
            _policyAgent.Dispose();
            _fastPolicyAgent.Dispose();
            _valueAgent.Dispose();
        }

        _disposed = true;
    }

    // TODO: improve to take only few moves with best probabilities - ranked moves
    private MovePrediction[] PredictMoves(IGameState gameState)
    {
        return [.._policyAgent.PredictMoves(gameState)];
    }

    private int PolicyRollout(IGameState gameState)
    {
        Player currentPlayer = gameState.NextPlayer;

        while (!gameState.IsOver())
        {
            Move move = _fastPolicyAgent.SelectMove(gameState);
            gameState = gameState.ApplyMove(move);
        }

        Player winner = gameState.GetWinner();

        if (winner == Player.None)
        {
            return 0;
        }

        return winner == currentPlayer ? 1 : -1;
    }

    private class AlphaGoNode(AlphaGoNode? parent = null, Move? move = null, float probability = 1.0f)
    {
        private readonly AlphaGoNode? _parent = parent;
        private AlphaGoNode[] _children = [];

        private int _visits = 0;
        private float _qValue = 0f;
        private float _uValue = probability;
        private float _priorValue = probability;

        public Move Move => move!;
        public AlphaGoNode[] Children => _children;
        public int Visits => _visits;

        public bool HasChildren => _children.Length > 0;

        public float QUValue => _qValue + _uValue;

        public AlphaGoNode SelectChild()
        {
            float bestValue = float.MinValue;
            AlphaGoNode? bestNode = null;

            for (int i = 0; i < _children.Length; i++)
            {
                AlphaGoNode child = _children[i];
                float quValue = child.QUValue;

                if (quValue > bestValue)
                {
                    bestValue = quValue;
                    bestNode = child;
                }
            }

            return bestNode!;

            // return _children.Aggregate((a, b) => a.QUValue > b.QUValue ? a : b);
        }

        public void ExpandChildren(MovePrediction[] moves)
        {
            _children = new AlphaGoNode[moves.Length];

            for (int i = 0; i < moves.Length; i++)
            {
                MovePrediction move = moves[i];
                _children[i] = new AlphaGoNode(this, move, move.Probability);
            }
        }

        public void UpdateValues(float value)
        {
            _parent?.UpdateValues(value);

            _visits++;

            _qValue += value / _visits;

            if (_parent is not null)
            {
                int cU = 5;
                _uValue = cU * _priorValue * (MathF.Sqrt(_parent._visits) / (1 + _visits));
            }
        }
    }
}

using Domain.Agents;
using Domain.Go;
using Domain.Encoders;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Infrastructure.Agents;

public enum PredictionType { Fast, Accurate, Sorted }

public class DeepLearningAgent(IEncoder encoder, string modelFile = "res_4_drop_5_10k_acc_463.onnx", float randomness = 0f, PredictionType predictionType = PredictionType.Fast) : IPolicyAgent
{
    private readonly InferenceSession _session = CreateSession(modelFile);
    private readonly IEncoder _encoder = encoder;
    private readonly float _randomness = randomness;
    private readonly PredictionType _predictionType = predictionType;
    private bool _disposed = false;

    public Move SelectMove(IGameState gameState)
    {
        return GetSortedMoves(gameState).FirstOrDefault(Move.Pass());
    }

    public IEnumerable<MovePrediction> PredictMoves(IGameState gameState)
    {
        return _predictionType switch
        {
            PredictionType.Fast => GetFastMoves(gameState),
            PredictionType.Accurate => GetAccurateMoves(gameState),
            PredictionType.Sorted => GetSortedMoves(gameState),
            _ => throw new NotImplementedException()
        };
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _session.Dispose();
        }

        _disposed = true;
    }

    private IEnumerable<MovePrediction> GetFastMoves(IGameState gameState)
    {
        float[] probabilities = Predict(gameState);

        foreach (Move move in gameState.GetValidMoves())
        {
            int moveIndex = _encoder.EncodePoint(move.Point);
            float probability = probabilities[moveIndex];

            if (_randomness > 0f)
            {
                probability *= _randomness * Random.Shared.NextSingle();
            }

            yield return new MovePrediction(move.Point, probability);
        }
    }

    private IEnumerable<MovePrediction> GetAccurateMoves(IGameState gameState)
    {
        float[] probabilities = Predict(gameState);
        Move[] moves = [.. gameState.GetValidMoves()];

        float[] moveProbabilities = new float[moves.Length];
        float moveProbabilitiesSum = 0f;

        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            int moveIndex = _encoder.EncodePoint(move.Point);
            float moveProbability = probabilities[moveIndex];

            if (_randomness > 0f)
            {
                moveProbability *= _randomness * Random.Shared.NextSingle();
            }

            moveProbability = MathF.Pow(moveProbability, 3);
            const float threshold = 1e-6f;
            moveProbability = Math.Clamp(moveProbability, threshold, 1f - threshold);

            moveProbabilities[i] = moveProbability;
            moveProbabilitiesSum += moveProbability;
        }

        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];
            float probability = moveProbabilities[i] / moveProbabilitiesSum;

            yield return new MovePrediction(move.Point, probability);
        }
    }

    private IEnumerable<MovePrediction> GetSortedMoves(IGameState gameState)
    {
        float[] probabilities = Predict(gameState);
        int[] probabilitiesIndices = new int[probabilities.Length];

        for (int i = 0; i < probabilities.Length; i++)
        {
            probabilitiesIndices[i] = i;
        }

        Array.Sort(probabilitiesIndices, (a, b) => probabilities[b].CompareTo(probabilities[a]));

        for (int i = 0; i < probabilitiesIndices.Length; i++)
        {
            int probabilityIndex = probabilitiesIndices[i];
            Point point = _encoder.DecodePointIndex(probabilityIndex);

            if (gameState.IsValidPoint(point).IsSuccess)
            {
                float probability = probabilities[probabilityIndex];

                if (_randomness > 0f)
                {
                    probability *= _randomness * Random.Shared.NextSingle();
                }

                yield return new MovePrediction(point, probability);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private float[] Predict(IGameState gameState)
    {
        ObjectDisposedException.ThrowIf(_disposed, _session);

        int[,,] encodedState = _encoder.Encode(gameState);
        float[] inputData = [.. encodedState];

        int[] shape = [1, .. _encoder.Shape];

        DenseTensor<float> inputTensor = new(inputData, shape);

        List<NamedOnnxValue> inputs = [NamedOnnxValue.CreateFromTensor(_session.InputMetadata.Keys.First(), inputTensor)];

        using var results = _session.Run(inputs);

        return [.. results[0].AsTensor<float>()];
    }

    private static InferenceSession CreateSession(string modelFile)
    {
        string policyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrainedModels", "Policy", modelFile);

        if (!File.Exists(policyPath))
        {
            throw new FileNotFoundException(policyPath);
        }

        return new InferenceSession(policyPath);
    }

    private static void PrintProbabilities(float[] probabilities)
    {
        Console.WriteLine($"Sum: {probabilities.Sum()}");
        Console.WriteLine($"Max: {probabilities.Max()}");

        int boardSize = 19;

        float[,] board = new float[boardSize, boardSize];

        for (int index = 0; index < probabilities.Length; index++)
        {
            int row = index / boardSize;
            int column = index % boardSize;

            board[row, column] = MathF.Round(probabilities[index], 4);
        }

        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                Console.Write($"{board[row, column]:F4}  ");
            }

            Console.WriteLine();
        }
    }
}

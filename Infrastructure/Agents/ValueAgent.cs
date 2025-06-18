using Domain.Agents;
using Domain.Encoders;
using Domain.Go;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Infrastructure.Agents;

public class ValueAgent(IEncoder encoder, string modelFile = "res_4_gap2d_20k_mse_226.onnx") : IValueAgent
{
    private readonly InferenceSession _session = CreateSession(modelFile);
    private bool _disposed = false;
    public IEncoder Encoder => encoder;

    public float Predict(IGameState gameState)
    {
        ObjectDisposedException.ThrowIf(_disposed, _session);

        float[] inputData = [.. Encoder.Encode(gameState)];
        int[] shape = [1, .. Encoder.Shape];

        DenseTensor<float> inputTensor = new(inputData, shape);

        List<NamedOnnxValue> inputs = [NamedOnnxValue.CreateFromTensor(_session.InputMetadata.Keys.First(), inputTensor)];

        using var results = _session.Run(inputs);

        return results[0].AsTensor<float>()[0];
    }

    public Move SelectMove(IGameState gameState)
    {
        Move[] validMoves = [.. gameState.GetValidMoves()];
        List<float> boards = [];

        for (int i = 0; i < validMoves.Length; i++)
        {
            Move move = validMoves[i];
            gameState = gameState.ApplyMove(move);
            boards.AddRange([.. Encoder.Encode(gameState)]);
        }

        float[] inputData = [.. boards];
        int[] shape = [validMoves.Length, .. Encoder.Shape];
        DenseTensor<float> inputTensor = new(inputData, shape);

        List<NamedOnnxValue> inputs = [NamedOnnxValue.CreateFromTensor(_session.InputMetadata.Keys.First(), inputTensor)];

        using var results = _session.Run(inputs);

        float[] opponentValues = results
            .SelectMany(result => result.AsTensor<float>().ToArray())
            .ToArray();

        float[] values = opponentValues.Select(value => -value).ToArray();

        int[] candidates = new int[values.Length];

        for (int i = 0; i < candidates.Length; i++)
            candidates[i] = i;

        Array.Sort(candidates, (a, b) => values[b].CompareTo(values[a]));

        for (int i = 0; i < candidates.Length; i++)
        {
            int candidatePointIndex = candidates[i];
            Point point = Encoder.DecodePointIndex(candidatePointIndex);

            if (gameState.IsValidPoint(point).IsSuccess)
            {
                return Move.Play(point);
            }
        }

        return Move.Pass();
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
            _session.Dispose();
        }
        
        _disposed = true;
    }

    private static InferenceSession CreateSession(string modelFile)
    {
        string policyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TrainedModels", "Value", modelFile);

        if (!File.Exists(policyPath))
        {
            throw new FileNotFoundException(policyPath);
        }

        return new InferenceSession(policyPath);
    }
}
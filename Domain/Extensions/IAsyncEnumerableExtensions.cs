namespace Domain.Extensions;

public static class IAsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<IEnumerable<TResult>> Batch<TResult>(this IAsyncEnumerable<TResult> source, int batchSize)
    {
        var batch = new List<TResult>(batchSize);

        await foreach (var item in source)
        {
            batch.Add(item);

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<TResult>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}

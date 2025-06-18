namespace Domain.Extensions;

public static class RandomExtensions
{
    // TODO: test which approach is faster - for both small and large samples
    public static T[] GetUniqueItems<T>(this T[] source, int samplesCount)
    {
        HashSet<T> randomUniqueItems = [];

        while (randomUniqueItems.Count < samplesCount)
        {
            int randomIndex = Random.Shared.Next(source.Length);
            T randomItem = source[randomIndex];
            randomUniqueItems.Add(randomItem);
        }

        return [.. randomUniqueItems];
    }

    public static T[] GetWeightedItems<T>(this T[] source, int samplesCount, float[] weights)
    {
        if (source.Length < samplesCount || weights.Length < samplesCount)
        {
            throw new IndexOutOfRangeException();
        }

        return source
            .Select((element, index) => (element, weight: weights[index]))
            .OrderByDescending(sample => Random.Shared.NextSingle() * sample.weight)
            .Take(samplesCount)
            .Select(sample => sample.element)
            .ToArray();
    }

    public static T[] GetShuffledItems<T>(this IEnumerable<T> source, int samplesCount)
    {
        T[] sourceArray = [..source];
        Random.Shared.Shuffle(sourceArray);
        return sourceArray[..samplesCount];
    }
}

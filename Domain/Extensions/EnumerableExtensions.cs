namespace Domain.Extensions;

public static class EnumerableExtensions
{
    public static int GetCombinedHashCode<T>(this IEnumerable<T> source) =>
        source.Aggregate(typeof(T).GetHashCode(), (hash, item) => HashCode.Combine(hash, item));

    /*
    // Fisher-Yates shuffle - already defined on Random.Shuffle
    public static T[] Shuffle<T>(this IEnumerable<T> source)
    {
        var array = source.ToArray();
        int count = array.Length;

        while (count > 1)
        {
            var i = Random.Shared.Next(count--);
            (array[i], array[count]) = (array[count], array[i]);
        }

        return array;
    }
    */
    
    public static T Pop<T>(this ICollection<T> source)
    {
        var lastElement = source.Last();
        source.Remove(lastElement);
        return lastElement;
    }

    public static T Pop<T>(this ICollection<T> source, int index)
    {
        T element = source.ElementAt(index);
        source.Remove(element);
        return element;
    }
}
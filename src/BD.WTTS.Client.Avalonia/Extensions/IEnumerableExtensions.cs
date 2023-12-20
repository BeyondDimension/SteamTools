namespace BD.WTTS;

/// <summary>
/// <see cref="IEnumerable"/> extensions methods
/// </summary>
public static class IEnumerableExtensions
{
    public static IEnumerable<IEnumerable> Batch(this IEnumerable source, int size)
    {
        object[]? bucket = null;
        var count = 0;

        foreach (var item in source)
        {
            bucket ??= new object[size];

            bucket[count++] = item;

            if (count == size)
            {
                yield return bucket;
                bucket = null;
                count = 0;
            }
        }

        if (bucket != null && count > 0)
            yield return bucket.Take(count).ToArray();
    }
}


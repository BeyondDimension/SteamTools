using System.Collections.Generic;

namespace System.Linq
{
    public static class Enumerable2
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T item) // https://github.com/iamandylamb/IEnumerable.Append/blob/master/src/Extension.cs
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection should not be null");
            }

            return collection.Concat(Enumerable.Repeat(item, 1));
        }
    }
}

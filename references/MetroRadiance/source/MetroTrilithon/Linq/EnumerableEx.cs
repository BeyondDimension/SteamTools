using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroTrilithon.Linq
{
	public static class EnumerableEx
	{
		public static IEnumerable<T> Return<T>(T value)
		{
			yield return value;
		}

		public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
				yield return item;
			}
		}

		public static string JoinString<T>(this IEnumerable<T> source, string separator)
		{
			return string.Join(separator, source is IEnumerable<string> ? (IEnumerable<string>)source : source.Select(x => x.ToString()));
		}
	}
}

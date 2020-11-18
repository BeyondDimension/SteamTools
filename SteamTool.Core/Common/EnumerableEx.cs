using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteamTool.Core.Common
{
	public static class EnumerableEx
	{
		public static IEnumerable<TResult> Return<TResult>(TResult value)
		{
			yield return value;
		}

		public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			var set = new HashSet<TKey>(EqualityComparer<TKey>.Default);

			foreach (var item in source)
			{
				var key = keySelector(item);
				if (set.Add(key)) yield return item;
			}
		}

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		/// <summary>
		/// コレクションを展開し、メンバーの文字列表現を指定した区切り文字で連結した文字列を返します。
		/// </summary>
		/// <typeparam name="T">コレクションに含まれる任意の型。</typeparam>
		/// <param name="source">対象のコレクション。</param>
		/// <param name="separator">セパレーターとして使用する文字列。</param>
		/// <returns>コレクションの文字列表現を展開し、指定したセパレーターで連結した文字列。</returns>
		public static string ToString<T>(this IEnumerable<T> source, string separator)
		{
			return string.Join(separator, source);
		}

		/// <summary>
		/// シーケンスが null でなく、1 つ以上の要素を含んでいるかどうかを確認します。
		/// </summary>
		public static bool HasItems<T>(this IEnumerable<T> source)
		{
			return source != null && source.Any();
		}
	}
}

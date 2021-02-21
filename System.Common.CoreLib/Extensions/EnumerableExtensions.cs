using System;
using System.Collections.Generic;
using System.Linq;
using System.Properties;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 通过使用相应类型的默认相等比较器对序列的元素进行比较，以确定两个序列是否相等
        /// </summary>
        /// <typeparam name="TSource">输入序列中的元素的类型</typeparam>
        /// <param name="first">一个用于比较 second 的 <see cref="IEnumerable{T}"/></param>
        /// <param name="second">要与第一个序列进行比较的 <see cref="IEnumerable{T}"/></param>
        /// <returns></returns>
        public static bool SequenceEqual_Nullable<TSource>(this IEnumerable<TSource>? first, IEnumerable<TSource>? second)
        {
            if (first == null) return second == null;
            if (second == null) return first == null;
            return first.SequenceEqual(second);
        }

        /// <summary>
        /// 确定序列是否包含任何元素
        /// </summary>
        /// <typeparam name="TSource">source 的元素类型</typeparam>
        /// <param name="source">要检查是否为空的 <see cref="IEnumerable{T}"/></param>
        /// <returns>如果源序列包含任何元素，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
        public static bool Any_Nullable<TSource>(this IEnumerable<TSource>? source)
        {
            if (source == null) return false;
            return source.Any();
        }

        /// <summary>
        /// 确定序列是否包含任何元素(带条件)
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static bool Any_Nullable<TSource>(this IEnumerable<TSource>? source, Func<TSource, bool> predicate)
        {
            if (source == null) return false;
            return source.Any(predicate);
        }

        public static Dictionary<TKey, TValue> ReverseKeyValue<TValue, TKey>(this IEnumerable<KeyValuePair<TValue, TKey>> source) where TKey : notnull
            => source.ToDictionary(k => k.Value, v => v.Key);

        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})"/>
        public static void AddRange<T>(this IList<T> ts, IEnumerable<T> collection)
        {
            if (ts is List<T> list)
            {
                list.AddRange(collection);
            }
            else
            {
                if (ts.IsReadOnly) throw new NotSupportedException(SR.NotSupported_FixedSizeCollection);
                var c = collection == ts ? collection.ToArray() : collection;
                foreach (var item in c)
                {
                    ts.Add(item);
                }
            }
        }
    }
}
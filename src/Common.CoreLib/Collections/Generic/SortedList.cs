using Nito.Comparers;
using System.Linq;
using ComparerBuilder_ = Nito.Comparers.ComparerBuilder;

namespace System.Collections.Generic
{
    public partial class SortedList<T> : IEnumerable, IEnumerable<T>
    {
        public static ComparerBuilderFor<T> ComparerBuilder => ComparerBuilder_.For<T>();

        SortedList<T, T> implement;

        public SortedList(IComparer<T> comparer)
        {
            implement = new SortedList<T, T>(comparer);
        }

        public SortedList(IComparer<T> comparer, IEnumerable<T> collection)
        {
            implement = new SortedList<T, T>(collection.ToDictionary(k => k, v => v), comparer);
        }

        public SortedList(IComparer<T> comparer, int capacity)
        {
            implement = new SortedList<T, T>(capacity, comparer);
        }

        public int Count => implement.Count;

        public IComparer<T> Comparer
        {
            get => implement.Comparer;
            set
            {
                if (value != implement.Comparer)
                {
                    implement = new SortedList<T, T>(implement, value);
                }
            }
        }

        public int Capacity { get => implement.Capacity; set => implement.Capacity = value; }

        public void Add(T item) => implement.Add(item, item);

        public void Clear() => implement.Clear();

        public bool Contains(T item) => implement.ContainsKey(item);

        public int IndexOf(T item) => implement.IndexOfKey(item);

        public bool Remove(T item) => implement.Remove(item);

        public void RemoveAt(int index) => implement.RemoveAt(index);

        public IEnumerable<T> AsEnumerable()
        {
            IEnumerable<T> result = this;
            return result;
        }

        public IEnumerator<T> GetEnumerator() => implement.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    partial class SortedList<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                ICollection<KeyValuePair<T, T>> collection = implement;
                return collection.IsReadOnly;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ICollection<T> collection = implement.Keys;
            collection.CopyTo(array, arrayIndex);
        }
    }

    partial class SortedList<T> : IList<T>, IReadOnlyList<T>
    {
        public T this[int index] { get => implement.Keys[index]; set => implement.Keys[index] = value; }

        public void Insert(int index, T item) => implement.Keys.Insert(index, item);
    }
}
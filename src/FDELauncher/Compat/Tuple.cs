// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/runtime/blob/v6.0.5/src/libraries/System.Private.CoreLib/src/System/Tuple.cs
// https://github.com/dotnet/runtime/blob/v6.0.5/src/libraries/System.Private.CoreLib/src/System/Collections/IStructuralComparable.cs
// https://github.com/dotnet/runtime/blob/v6.0.5/src/libraries/System.Private.CoreLib/src/System/Collections/IStructuralEquatable.cs

#if NET35
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace System
{
    /// <summary>
    /// Helper so we can call some tuple methods recursively without knowing the underlying types.
    /// </summary>
    internal interface ITupleInternal : ITuple
    {
        string ToString(StringBuilder sb);
        int GetHashCode(IEqualityComparer comparer);
    }

    public static class Tuple
    {
        public static Tuple<T1> Create<T1>(T1 item1)
        {
            return new Tuple<T1>(item1);
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }

        public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            return new Tuple<T1, T2, T3, T4>(item1, item2, item3, item4);
        }

        public static Tuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            return new Tuple<T1, T2, T3, T4, T5>(item1, item2, item3, item4, item5);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6>(item1, item2, item3, item4, item5, item6);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>(item1, item2, item3, item4, item5, item6, item7);
        }

        public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
        {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>>(item1, item2, item3, item4, item5, item6, item7, new Tuple<T8>(item8));
        }

        // Note: F# compiler depends on the exact tuple hashing algorithm. Do not ever change it.

        // From System.Web.Util.HashCodeCombiner
        internal static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }

        internal static int CombineHashCodes(int h1, int h2, int h3)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6));
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7));
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7, h8));
        }
    }

    [Serializable]
    public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
    {
        private readonly T1 m_Item1; // Do not rename (binary serialization)

        public T1 Item1 => m_Item1;

        public Tuple(T1 item1)
        {
            m_Item1 = item1;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            return Equals(other, comparer);
        }

        private bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            if (other is not Tuple<T1> objTuple)
            {
                return false;
            }

            return comparer.Equals(m_Item1, objTuple.m_Item1);
        }

        int IComparable.CompareTo(object? obj)
        {
            return CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            return CompareTo(other, comparer);
        }

        private int CompareTo(object? other, IComparer comparer)
        {
            if (other == null) return 1;

            if (other is not Tuple<T1> objTuple)
            {
                throw new ArgumentException(string.Format(SR.ArgumentException_TupleIncorrectType, GetType()), nameof(other));
            }

            return comparer.Compare(m_Item1, objTuple.m_Item1);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        int ITupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(m_Item1!);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ToString(sb);
        }

        string ITupleInternal.ToString(StringBuilder sb)
        {
            return ToString(sb);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(m_Item1);
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int ITuple.Length => 1;

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? ITuple.this[int index]
        {
            get
            {
                if (index != 0)
                {
                    throw new IndexOutOfRangeException();
                }
                return Item1;
            }
        }
    }

    [Serializable]
    public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
    {
        private readonly T1 m_Item1; // Do not rename (binary serialization)
        private readonly T2 m_Item2; // Do not rename (binary serialization)

        public T1 Item1 => m_Item1;
        public T2 Item2 => m_Item2;

        public Tuple(T1 item1, T2 item2)
        {
            m_Item1 = item1;
            m_Item2 = item2;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            return Equals(other, comparer);
        }

        private bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            if (other is not Tuple<T1, T2> objTuple)
            {
                return false;
            }

            return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2);
        }

        int IComparable.CompareTo(object? obj)
        {
            return CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            return CompareTo(other, comparer);
        }

        private int CompareTo(object? other, IComparer comparer)
        {
            if (other == null) return 1;

            if (other is not Tuple<T1, T2> objTuple)
            {
                throw new ArgumentException(string.Format(SR.ArgumentException_TupleIncorrectType, GetType()), nameof(other));
            }

            int c = comparer.Compare(m_Item1, objTuple.m_Item1);

            if (c != 0) return c;

            return comparer.Compare(m_Item2, objTuple.m_Item2);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        int ITupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1!), comparer.GetHashCode(m_Item2!));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ToString(sb);
        }

        string ITupleInternal.ToString(StringBuilder sb)
        {
            return ToString(sb);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int ITuple.Length => 2;

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? ITuple.this[int index] =>
            index switch
            {
                0 => Item1,
                1 => Item2,
                _ => throw new IndexOutOfRangeException(),
            };
    }

    [Serializable]
    public class Tuple<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
    {
        private readonly T1 m_Item1; // Do not rename (binary serialization)
        private readonly T2 m_Item2; // Do not rename (binary serialization)
        private readonly T3 m_Item3; // Do not rename (binary serialization)

        public T1 Item1 => m_Item1;
        public T2 Item2 => m_Item2;
        public T3 Item3 => m_Item3;

        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            return Equals(other, comparer);
        }

        private bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            if (other is not Tuple<T1, T2, T3> objTuple)
            {
                return false;
            }

            return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2) && comparer.Equals(m_Item3, objTuple.m_Item3);
        }

        int IComparable.CompareTo(object? obj)
        {
            return CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            return CompareTo(other, comparer);
        }

        private int CompareTo(object? other, IComparer comparer)
        {
            if (other == null) return 1;

            if (other is not Tuple<T1, T2, T3> objTuple)
            {
                throw new ArgumentException(string.Format(SR.ArgumentException_TupleIncorrectType, GetType()), nameof(other));
            }

            int c = comparer.Compare(m_Item1, objTuple.m_Item1);

            if (c != 0) return c;

            c = comparer.Compare(m_Item2, objTuple.m_Item2);

            if (c != 0) return c;

            return comparer.Compare(m_Item3, objTuple.m_Item3);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        int ITupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1!), comparer.GetHashCode(m_Item2!), comparer.GetHashCode(m_Item3!));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ToString(sb);
        }

        string ITupleInternal.ToString(StringBuilder sb)
        {
            return ToString(sb);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int ITuple.Length => 3;

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? ITuple.this[int index] =>
            index switch
            {
                0 => Item1,
                1 => Item2,
                2 => Item3,
                _ => throw new IndexOutOfRangeException(),
            };
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
    {
        private readonly T1 m_Item1; // Do not rename (binary serialization)
        private readonly T2 m_Item2; // Do not rename (binary serialization)
        private readonly T3 m_Item3; // Do not rename (binary serialization)
        private readonly T4 m_Item4; // Do not rename (binary serialization)

        public T1 Item1 => m_Item1;
        public T2 Item2 => m_Item2;
        public T3 Item3 => m_Item3;
        public T4 Item4 => m_Item4;

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            return Equals(other, comparer);
        }

        private bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            if (other is not Tuple<T1, T2, T3, T4> objTuple)
            {
                return false;
            }

            return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2) && comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4);
        }

        int IComparable.CompareTo(object? obj)
        {
            return CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            return CompareTo(other, comparer);
        }

        private int CompareTo(object? other, IComparer comparer)
        {
            if (other == null) return 1;

            if (other is not Tuple<T1, T2, T3, T4> objTuple)
            {
                throw new ArgumentException(string.Format(SR.ArgumentException_TupleIncorrectType, GetType()), nameof(other));
            }

            int c = comparer.Compare(m_Item1, objTuple.m_Item1);

            if (c != 0) return c;

            c = comparer.Compare(m_Item2, objTuple.m_Item2);

            if (c != 0) return c;

            c = comparer.Compare(m_Item3, objTuple.m_Item3);

            if (c != 0) return c;

            return comparer.Compare(m_Item4, objTuple.m_Item4);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        int ITupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1!), comparer.GetHashCode(m_Item2!), comparer.GetHashCode(m_Item3!), comparer.GetHashCode(m_Item4!));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ToString(sb);
        }

        string ITupleInternal.ToString(StringBuilder sb)
        {
            return ToString(sb);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int ITuple.Length => 4;

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? ITuple.this[int index] =>
            index switch
            {
                0 => Item1,
                1 => Item2,
                2 => Item3,
                3 => Item4,
                _ => throw new IndexOutOfRangeException(),
            };
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
    {
        private readonly T1 m_Item1; // Do not rename (binary serialization)
        private readonly T2 m_Item2; // Do not rename (binary serialization)
        private readonly T3 m_Item3; // Do not rename (binary serialization)
        private readonly T4 m_Item4; // Do not rename (binary serialization)
        private readonly T5 m_Item5; // Do not rename (binary serialization)

        public T1 Item1 => m_Item1;
        public T2 Item2 => m_Item2;
        public T3 Item3 => m_Item3;
        public T4 Item4 => m_Item4;
        public T5 Item5 => m_Item5;

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
            m_Item5 = item5;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            return Equals(other, comparer);
        }

        private bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            if (other is not Tuple<T1, T2, T3, T4, T5> objTuple)
            {
                return false;
            }

            return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2) && comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4) && comparer.Equals(m_Item5, objTuple.m_Item5);
        }

        int IComparable.CompareTo(object? obj)
        {
            return CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            return CompareTo(other, comparer);
        }

        private int CompareTo(object? other, IComparer comparer)
        {
            if (other == null) return 1;

            if (other is not Tuple<T1, T2, T3, T4, T5> objTuple)
            {
                throw new ArgumentException(string.Format(SR.ArgumentException_TupleIncorrectType, GetType()), nameof(other));
            }

            int c = comparer.Compare(m_Item1, objTuple.m_Item1);

            if (c != 0) return c;

            c = comparer.Compare(m_Item2, objTuple.m_Item2);

            if (c != 0) return c;

            c = comparer.Compare(m_Item3, objTuple.m_Item3);

            if (c != 0) return c;

            c = comparer.Compare(m_Item4, objTuple.m_Item4);

            if (c != 0) return c;

            return comparer.Compare(m_Item5, objTuple.m_Item5);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        int ITupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1!), comparer.GetHashCode(m_Item2!), comparer.GetHashCode(m_Item3!), comparer.GetHashCode(m_Item4!), comparer.GetHashCode(m_Item5!));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ToString(sb);
        }

        string ITupleInternal.ToString(StringBuilder sb)
        {
            return ToString(sb);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(", ");
            sb.Append(m_Item5);
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int ITuple.Length => 5;

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? ITuple.this[int index] =>
            index switch
            {
                0 => Item1,
                1 => Item2,
                2 => Item3,
                3 => Item4,
                4 => Item5,
                _ => throw new IndexOutOfRangeException(),
            };
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5, T6> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
    {
        private readonly T1 m_Item1; // Do not rename (binary serialization)
        private readonly T2 m_Item2; // Do not rename (binary serialization)
        private readonly T3 m_Item3; // Do not rename (binary serialization)
        private readonly T4 m_Item4; // Do not rename (binary serialization)
        private readonly T5 m_Item5; // Do not rename (binary serialization)
        private readonly T6 m_Item6; // Do not rename (binary serialization)

        public T1 Item1 => m_Item1;
        public T2 Item2 => m_Item2;
        public T3 Item3 => m_Item3;
        public T4 Item4 => m_Item4;
        public T5 Item5 => m_Item5;
        public T6 Item6 => m_Item6;

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
            m_Item5 = item5;
            m_Item6 = item6;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            return Equals(other, comparer);
        }

        private bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            if (other is not Tuple<T1, T2, T3, T4, T5, T6> objTuple)
            {
                return false;
            }

            return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2) && comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4) && comparer.Equals(m_Item5, objTuple.m_Item5) && comparer.Equals(m_Item6, objTuple.m_Item6);
        }

        int IComparable.CompareTo(object? obj)
        {
            return CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            return CompareTo(other, comparer);
        }

        private int CompareTo(object? other, IComparer comparer)
        {
            if (other == null) return 1;

            if (other is not Tuple<T1, T2, T3, T4, T5, T6> objTuple)
            {
                throw new ArgumentException(string.Format(SR.ArgumentException_TupleIncorrectType, GetType()), nameof(other));
            }

            int c = comparer.Compare(m_Item1, objTuple.m_Item1);

            if (c != 0) return c;

            c = comparer.Compare(m_Item2, objTuple.m_Item2);

            if (c != 0) return c;

            c = comparer.Compare(m_Item3, objTuple.m_Item3);

            if (c != 0) return c;

            c = comparer.Compare(m_Item4, objTuple.m_Item4);

            if (c != 0) return c;

            c = comparer.Compare(m_Item5, objTuple.m_Item5);

            if (c != 0) return c;

            return comparer.Compare(m_Item6, objTuple.m_Item6);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        int ITupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1!), comparer.GetHashCode(m_Item2!), comparer.GetHashCode(m_Item3!), comparer.GetHashCode(m_Item4!), comparer.GetHashCode(m_Item5!), comparer.GetHashCode(m_Item6!));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ToString(sb);
        }

        string ITupleInternal.ToString(StringBuilder sb)
        {
            return ToString(sb);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(", ");
            sb.Append(m_Item5);
            sb.Append(", ");
            sb.Append(m_Item6);
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int ITuple.Length => 6;

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? ITuple.this[int index] =>
            index switch
            {
                0 => Item1,
                1 => Item2,
                2 => Item3,
                3 => Item4,
                4 => Item5,
                5 => Item6,
                _ => throw new IndexOutOfRangeException(),
            };
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5, T6, T7> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
    {
        private readonly T1 m_Item1; // Do not rename (binary serialization)
        private readonly T2 m_Item2; // Do not rename (binary serialization)
        private readonly T3 m_Item3; // Do not rename (binary serialization)
        private readonly T4 m_Item4; // Do not rename (binary serialization)
        private readonly T5 m_Item5; // Do not rename (binary serialization)
        private readonly T6 m_Item6; // Do not rename (binary serialization)
        private readonly T7 m_Item7; // Do not rename (binary serialization)

        public T1 Item1 => m_Item1;
        public T2 Item2 => m_Item2;
        public T3 Item3 => m_Item3;
        public T4 Item4 => m_Item4;
        public T5 Item5 => m_Item5;
        public T6 Item6 => m_Item6;
        public T7 Item7 => m_Item7;

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
            m_Item5 = item5;
            m_Item6 = item6;
            m_Item7 = item7;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            return Equals(other, comparer);
        }

        private bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            if (other is not Tuple<T1, T2, T3, T4, T5, T6, T7> objTuple)
            {
                return false;
            }

            return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2) && comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4) && comparer.Equals(m_Item5, objTuple.m_Item5) && comparer.Equals(m_Item6, objTuple.m_Item6) && comparer.Equals(m_Item7, objTuple.m_Item7);
        }

        int IComparable.CompareTo(object? obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            return CompareTo(other, comparer);
        }

        private int CompareTo(object? other, IComparer comparer)
        {
            if (other == null) return 1;

            if (other is not Tuple<T1, T2, T3, T4, T5, T6, T7> objTuple)
            {
                throw new ArgumentException(string.Format(SR.ArgumentException_TupleIncorrectType, GetType()), nameof(other));
            }

            int c = comparer.Compare(m_Item1, objTuple.m_Item1);

            if (c != 0) return c;

            c = comparer.Compare(m_Item2, objTuple.m_Item2);

            if (c != 0) return c;

            c = comparer.Compare(m_Item3, objTuple.m_Item3);

            if (c != 0) return c;

            c = comparer.Compare(m_Item4, objTuple.m_Item4);

            if (c != 0) return c;

            c = comparer.Compare(m_Item5, objTuple.m_Item5);

            if (c != 0) return c;

            c = comparer.Compare(m_Item6, objTuple.m_Item6);

            if (c != 0) return c;

            return comparer.Compare(m_Item7, objTuple.m_Item7);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        int ITupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1!), comparer.GetHashCode(m_Item2!), comparer.GetHashCode(m_Item3!), comparer.GetHashCode(m_Item4!), comparer.GetHashCode(m_Item5!), comparer.GetHashCode(m_Item6!), comparer.GetHashCode(m_Item7!));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ToString(sb);
        }

        string ITupleInternal.ToString(StringBuilder sb)
        {
            return ToString(sb);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(", ");
            sb.Append(m_Item5);
            sb.Append(", ");
            sb.Append(m_Item6);
            sb.Append(", ");
            sb.Append(m_Item7);
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int ITuple.Length => 7;

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? ITuple.this[int index] =>
            index switch
            {
                0 => Item1,
                1 => Item2,
                2 => Item3,
                3 => Item4,
                4 => Item5,
                5 => Item6,
                6 => Item7,
                _ => throw new IndexOutOfRangeException(),
            };
    }

    [Serializable]
    public class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple where TRest : notnull
    {
        private readonly T1 m_Item1; // Do not rename (binary serialization)
        private readonly T2 m_Item2; // Do not rename (binary serialization)
        private readonly T3 m_Item3; // Do not rename (binary serialization)
        private readonly T4 m_Item4; // Do not rename (binary serialization)
        private readonly T5 m_Item5; // Do not rename (binary serialization)
        private readonly T6 m_Item6; // Do not rename (binary serialization)
        private readonly T7 m_Item7; // Do not rename (binary serialization)
        private readonly TRest m_Rest; // Do not rename (binary serialization)

        public T1 Item1 => m_Item1;
        public T2 Item2 => m_Item2;
        public T3 Item3 => m_Item3;
        public T4 Item4 => m_Item4;
        public T5 Item5 => m_Item5;
        public T6 Item6 => m_Item6;
        public T7 Item7 => m_Item7;
        public TRest Rest => m_Rest;

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
        {
            if (rest is not ITupleInternal)
            {
                throw new ArgumentException(SR.ArgumentException_TupleLastArgumentNotATuple);
            }

            m_Item1 = item1;
            m_Item2 = item2;
            m_Item3 = item3;
            m_Item4 = item4;
            m_Item5 = item5;
            m_Item6 = item6;
            m_Item7 = item7;
            m_Rest = rest;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            return Equals(other, comparer);
        }

        private bool Equals([NotNullWhen(true)] object? other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            if (other is not Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> objTuple)
            {
                return false;
            }

            return comparer.Equals(m_Item1, objTuple.m_Item1) && comparer.Equals(m_Item2, objTuple.m_Item2) && comparer.Equals(m_Item3, objTuple.m_Item3) && comparer.Equals(m_Item4, objTuple.m_Item4) && comparer.Equals(m_Item5, objTuple.m_Item5) && comparer.Equals(m_Item6, objTuple.m_Item6) && comparer.Equals(m_Item7, objTuple.m_Item7) && comparer.Equals(m_Rest, objTuple.m_Rest);
        }

        int IComparable.CompareTo(object? obj)
        {
            return CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object? other, IComparer comparer)
        {
            return CompareTo(other, comparer);
        }

        private int CompareTo(object? other, IComparer comparer)
        {
            if (other == null) return 1;

            if (other is not Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> objTuple)
            {
                throw new ArgumentException(string.Format(SR.ArgumentException_TupleIncorrectType, GetType()), nameof(other));
            }

            int c = comparer.Compare(m_Item1, objTuple.m_Item1);

            if (c != 0) return c;

            c = comparer.Compare(m_Item2, objTuple.m_Item2);

            if (c != 0) return c;

            c = comparer.Compare(m_Item3, objTuple.m_Item3);

            if (c != 0) return c;

            c = comparer.Compare(m_Item4, objTuple.m_Item4);

            if (c != 0) return c;

            c = comparer.Compare(m_Item5, objTuple.m_Item5);

            if (c != 0) return c;

            c = comparer.Compare(m_Item6, objTuple.m_Item6);

            if (c != 0) return c;

            c = comparer.Compare(m_Item7, objTuple.m_Item7);

            if (c != 0) return c;

            return comparer.Compare(m_Rest, objTuple.m_Rest);
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        private int GetHashCode(IEqualityComparer comparer)
        {
            // We want to have a limited hash in this case.  We'll use the last 8 elements of the tuple
            ITupleInternal t = (ITupleInternal)m_Rest;
            if (t.Length >= 8) { return t.GetHashCode(comparer); }

            // In this case, the m_Rest member has fewer than 8 elements so we need to combine our elements with the elements in m_Rest.
            int k = 8 - t.Length;
            switch (k)
            {
                case 1:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item7!), t.GetHashCode(comparer));
                case 2:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item6!), comparer.GetHashCode(m_Item7!), t.GetHashCode(comparer));
                case 3:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item5!), comparer.GetHashCode(m_Item6!), comparer.GetHashCode(m_Item7!), t.GetHashCode(comparer));
                case 4:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item4!), comparer.GetHashCode(m_Item5!), comparer.GetHashCode(m_Item6!), comparer.GetHashCode(m_Item7!), t.GetHashCode(comparer));
                case 5:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item3!), comparer.GetHashCode(m_Item4!), comparer.GetHashCode(m_Item5!), comparer.GetHashCode(m_Item6!), comparer.GetHashCode(m_Item7!), t.GetHashCode(comparer));
                case 6:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item2!), comparer.GetHashCode(m_Item3!), comparer.GetHashCode(m_Item4!), comparer.GetHashCode(m_Item5!), comparer.GetHashCode(m_Item6!), comparer.GetHashCode(m_Item7!), t.GetHashCode(comparer));
                case 7:
                    return Tuple.CombineHashCodes(comparer.GetHashCode(m_Item1!), comparer.GetHashCode(m_Item2!), comparer.GetHashCode(m_Item3!), comparer.GetHashCode(m_Item4!), comparer.GetHashCode(m_Item5!), comparer.GetHashCode(m_Item6!), comparer.GetHashCode(m_Item7!), t.GetHashCode(comparer));
            }
            Debug.Fail("Missed all cases for computing Tuple hash code");
            return -1;
        }

        int ITupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCode(comparer);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('(');
            return ToString(sb);
        }

        string ITupleInternal.ToString(StringBuilder sb)
        {
            return ToString(sb);
        }

        private string ToString(StringBuilder sb)
        {
            sb.Append(m_Item1);
            sb.Append(", ");
            sb.Append(m_Item2);
            sb.Append(", ");
            sb.Append(m_Item3);
            sb.Append(", ");
            sb.Append(m_Item4);
            sb.Append(", ");
            sb.Append(m_Item5);
            sb.Append(", ");
            sb.Append(m_Item6);
            sb.Append(", ");
            sb.Append(m_Item7);
            sb.Append(", ");
            return ((ITupleInternal)m_Rest).ToString(sb);
        }

        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int ITuple.Length => 7 + ((ITupleInternal)Rest).Length;

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? ITuple.this[int index] =>
            index switch
            {
                0 => Item1,
                1 => Item2,
                2 => Item3,
                3 => Item4,
                4 => Item5,
                5 => Item6,
                6 => Item7,

                _ => ((ITupleInternal)Rest)[index - 7],
            };
    }
}

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// This interface is required for types that want to be indexed into by dynamic patterns.
    /// </summary>
    public interface ITuple
    {
        /// <summary>
        /// The number of positions in this data structure.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Get the element at position <param name="index"/>.
        /// </summary>
        object? this[int index] { get; }
    }
}

namespace System.Collections
{
    public interface IStructuralComparable
    {
        int CompareTo(object? other, IComparer comparer);
    }

    public interface IStructuralEquatable
    {
        bool Equals(object? other, IEqualityComparer comparer);
        int GetHashCode(IEqualityComparer comparer);
    }
}
#endif
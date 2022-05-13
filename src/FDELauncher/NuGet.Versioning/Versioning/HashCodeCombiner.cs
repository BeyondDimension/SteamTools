// https://github.com/NuGetArchive/NuGet.Versioning/blob/release/src/Versioning/HashCodeCombiner.cs

namespace NuGet.Versioning
{
    /// <summary>
    /// Hash code creator, based on the original NuGet hash code combiner/ASP hash code combiner implementations
    /// </summary>
    internal sealed class HashCodeCombiner
    {
        // seed from String.GetHashCode()
        private const long Seed = 0x1505L;

        private long _combinedHash;

        internal HashCodeCombiner()
        {
            _combinedHash = Seed;
        }

        internal int CombinedHash
        {
            get { return _combinedHash.GetHashCode(); }
        }

        internal void AddInt32(int i)
        {
            _combinedHash = ((_combinedHash << 5) + _combinedHash) ^ i;
        }

        internal void AddObject(int i)
        {
            AddInt32(i);
        }

        internal void AddObject(bool b)
        {
            AddInt32(b.GetHashCode());
        }

        internal void AddObject(object? o)
        {
            if (o != null)
            {
                AddInt32(o.GetHashCode());
            }
        }

        /// <summary>
        /// Create a unique hash code for the given set of items
        /// </summary>
        internal static int GetHashCode(params object[] objects)
        {
            HashCodeCombiner combiner = new HashCodeCombiner();

            foreach (object obj in objects)
            {
                combiner.AddObject(obj);
            }

            return combiner.CombinedHash;
        }
    }
}
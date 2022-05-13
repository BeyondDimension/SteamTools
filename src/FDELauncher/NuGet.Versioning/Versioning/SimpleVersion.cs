// https://github.com/NuGetArchive/NuGet.Versioning/blob/release/src/Versioning/SimpleVersion.cs

using System;

namespace NuGet.Versioning
{
    /// <summary>
    /// A basic version that allows comparisons.
    /// </summary>
    public abstract class SimpleVersion : IFormattable, IComparable, IComparable<SimpleVersion>, IEquatable<SimpleVersion>
    {
        /// <summary>
        /// Gives a normalized representation of the version.
        /// </summary>
        public virtual string ToNormalizedString()
        {
            return ToString("N", new VersionFormatter());
        }

        public override string ToString()
        {
            return ToNormalizedString();
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            string? formattedString;

            if (formatProvider == null || !TryFormatter(format, formatProvider, out formattedString))
            {
                formattedString = ToString();
            }

            return formattedString ?? string.Empty;
        }

        protected bool TryFormatter(string format, IFormatProvider formatProvider, out string? formattedString)
        {
            bool formatted = false;
            formattedString = null;

            if (formatProvider != null)
            {
                if (formatProvider.GetFormat(GetType()) is ICustomFormatter formatter)
                {
                    formatted = true;
                    formattedString = formatter.Format(format, this, formatProvider);
                }
            }

            return formatted;
        }

        public override int GetHashCode()
        {
            return VersionComparer.Default.GetHashCode(this);
        }

        public virtual int CompareTo(object obj)
        {
            return CompareTo(obj as SimpleVersion);
        }

        public virtual int CompareTo(SimpleVersion? other)
        {
            return CompareTo(other, VersionComparison.Default);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SimpleVersion);
        }

        public virtual bool Equals(SimpleVersion? other)
        {
            return Equals(other, VersionComparison.Default);
        }

        /// <summary>
        /// True if the VersionBase objects are equal based on the given comparison mode.
        /// </summary>
        public virtual bool Equals(SimpleVersion? other, VersionComparison versionComparison)
        {
            return CompareTo(other, versionComparison) == 0;
        }

        /// <summary>
        /// Compares NuGetVersion objects using the given comparison mode.
        /// </summary>
        public virtual int CompareTo(SimpleVersion? other, VersionComparison versionComparison)
        {
            VersionComparer comparer = new VersionComparer(versionComparison);
            return comparer.Compare(this, other);
        }

        /// <summary>
        /// ==
        /// </summary>
        public static bool operator ==(SimpleVersion? version1, SimpleVersion? version2)
        {
            return Compare(version1, version2) == 0;
        }

        /// <summary>
        /// !=
        /// </summary>
        public static bool operator !=(SimpleVersion? version1, SimpleVersion? version2)
        {
            return Compare(version1, version2) != 0;
        }

        /// <summary>
        /// <
        /// </summary>
        public static bool operator <(SimpleVersion? version1, SimpleVersion? version2)
        {
            return Compare(version1, version2) < 0;
        }

        /// <summary>
        /// <=
        /// </summary>
        public static bool operator <=(SimpleVersion? version1, SimpleVersion? version2)
        {
            return Compare(version1, version2) <= 0;
        }

        /// <summary>
        /// >
        /// </summary>
        public static bool operator >(SimpleVersion? version1, SimpleVersion? version2)
        {
            return Compare(version1, version2) > 0;
        }

        /// <summary>
        /// >=
        /// </summary>
        public static bool operator >=(SimpleVersion? version1, SimpleVersion? version2)
        {
            return Compare(version1, version2) >= 0;
        }

        private static int Compare(SimpleVersion? version1, SimpleVersion? version2)
        {
            IVersionComparer comparer = new VersionComparer();
            return comparer.Compare(version1!, version2!);
        }
    }
}
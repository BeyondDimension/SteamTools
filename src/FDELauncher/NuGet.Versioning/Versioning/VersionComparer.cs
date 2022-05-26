// https://github.com/NuGetArchive/NuGet.Versioning/blob/release/src/Versioning/VersionComparer.cs

using System.Globalization;

namespace NuGet.Versioning
{
    /// <summary>
    /// An IVersionComparer for NuGetVersion and NuGetVersion types.
    /// </summary>
    public sealed class VersionComparer : IVersionComparer
    {
        private VersionComparison _mode;

        /// <summary>
        /// Creates a VersionComparer using the default mode.
        /// </summary>
        public VersionComparer()
        {
            _mode = VersionComparison.Default;
        }

        /// <summary>
        /// Creates a VersionComparer that respects the given comparison mode.
        /// </summary>
        /// <param name="versionComparison">comparison mode</param>
        public VersionComparer(VersionComparison versionComparison)
        {
            _mode = versionComparison;
        }

        /// <summary>
        /// Determines if both versions are equal.
        /// </summary>
        public bool Equals(SimpleVersion x, SimpleVersion y)
        {
            return Compare(x, y) == 0;
        }

        /// <summary>
        /// Compares the given versions using the VersionComparison mode.
        /// </summary>
        public static int Compare(SimpleVersion version1, SimpleVersion version2, VersionComparison versionComparison)
        {
            IVersionComparer comparer = new VersionComparer(versionComparison);
            return comparer.Compare(version1, version2);
        }

        /// <summary>
        /// Gives a hash code based on the normalized version string.
        /// </summary>
        public int GetHashCode(SimpleVersion obj)
        {
            if (obj is null)
            {
                return 0;
            }

            HashCodeCombiner combiner = new HashCodeCombiner();

            var semVersion = obj as SemanticVersion;
            var nuGetVersion = obj as NuGetVersion;

            if (semVersion != null)
            {
                combiner.AddObject(semVersion.Major);
                combiner.AddObject(semVersion.Minor);
                combiner.AddObject(semVersion.Patch);

                if (nuGetVersion != null && nuGetVersion.Revision > 0)
                {
                    combiner.AddObject(nuGetVersion.Revision);
                }

                if (_mode == VersionComparison.Default || _mode == VersionComparison.VersionRelease || _mode == VersionComparison.VersionReleaseMetadata)
                {
                    if (semVersion.IsPrerelease)
                    {
                        combiner.AddObject(semVersion.Release.ToUpperInvariant());
                    }
                }

                if (_mode == VersionComparison.VersionReleaseMetadata)
                {
                    if (semVersion.HasMetadata)
                    {
                        combiner.AddObject(semVersion.Metadata);
                    }
                }

                return combiner.CombinedHash;
            }
            else
            {
                // This is a new kind of version, fall back to using the formatter
                string verString = string.Empty;

                VersionFormatter formatter = new VersionFormatter();

                if (_mode == VersionComparison.Default || _mode == VersionComparison.VersionRelease)
                {
                    verString = obj.ToString("V-R", formatter).ToUpperInvariant();
                }
                else if (_mode == VersionComparison.Version)
                {
                    verString = obj.ToString("V", formatter);
                }
                else if (_mode == VersionComparison.VersionReleaseMetadata)
                {
                    verString = string.Format(CultureInfo.InvariantCulture, "{0}+{1}",
                        obj.ToString("V-R", formatter).ToUpperInvariant(),
                        obj.ToString("M", formatter));
                }

                if (string.IsNullOrEmpty(verString))
                {
                    verString = obj.ToNormalizedString().ToUpperInvariant();
                }

                combiner.AddObject(verString);
            }

            return combiner.CombinedHash;
        }

        /// <summary>
        /// Compare versions.
        /// </summary>
        public int Compare(SimpleVersion? x, SimpleVersion? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            // null checks
            if (x is null && y is null)
            {
                return 0;
            }

            if (y is null)
            {
                return 1;
            }

            if (x is null)
            {
                return -1;
            }

            var semX = x as SemanticVersion;
            var semY = y as SemanticVersion;

            if (semX != null && semY != null)
            {
                // compare version
                int result = semX.Major.CompareTo(semY.Major);
                if (result != 0)
                    return result;

                result = semX.Minor.CompareTo(semY.Minor);
                if (result != 0)
                    return result;

                result = semX.Patch.CompareTo(semY.Patch);
                if (result != 0)
                    return result;

                var legacyX = x as NuGetVersion;
                var legacyY = y as NuGetVersion;

                result = CompareLegacyVersion(legacyX, legacyY);
                if (result != 0)
                    return result;

                if (_mode != VersionComparison.Version)
                {
                    // compare release labels
                    if (semX.IsPrerelease && !semY.IsPrerelease)
                        return -1;

                    if (!semX.IsPrerelease && semY.IsPrerelease)
                        return 1;

                    if (semX.IsPrerelease && semY.IsPrerelease)
                    {
                        result = CompareReleaseLabels(semX.ReleaseLabels, semY.ReleaseLabels);
                        if (result != 0)
                            return result;
                    }

                    // compare the metadata
                    if (_mode == VersionComparison.VersionReleaseMetadata)
                    {
                        result = StringComparer.OrdinalIgnoreCase.Compare(semX.Metadata ?? string.Empty, semY.Metadata ?? string.Empty);
                        if (result != 0)
                            return result;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Compares the 4th digit of the version number.
        /// </summary>
        private static int CompareLegacyVersion(NuGetVersion? legacyX, NuGetVersion? legacyY)
        {
            int result = 0;

            // true if one has a 4th version number
            if (legacyX != null && legacyY != null)
            {
                result = legacyX.Version.CompareTo(legacyY.Version);
            }
            else if (legacyX != null && legacyX.Version.Revision > 0)
            {
                result = 1;
            }
            else if (legacyY != null && legacyY.Version.Revision > 0)
            {
                result = -1;
            }

            return result;
        }

        /// <summary>
        /// A default comparer that compares metadata as strings.
        /// </summary>
        public static IVersionComparer Default
        {
            get
            {
                return new VersionComparer(VersionComparison.Default);
            }
        }

        /// <summary>
        /// A comparer that uses only the version numbers.
        /// </summary>
        public static IVersionComparer Version
        {
            get
            {
                return new VersionComparer(VersionComparison.Version);
            }
        }

        /// <summary>
        /// Compares versions without comparing the metadata.
        /// </summary>
        public static IVersionComparer VersionRelease
        {
            get
            {
                return new VersionComparer(VersionComparison.VersionRelease);
            }
        }

        /// <summary>
        /// A version comparer that follows SemVer 2.0.0 rules.
        /// </summary>
        public static IVersionComparer VersionReleaseMetadata
        {
            get
            {
                return new VersionComparer(VersionComparison.VersionReleaseMetadata);
            }
        }

        /// <summary>
        /// Compares sets of release labels.
        /// </summary>
        private static int CompareReleaseLabels(IEnumerable<string> version1, IEnumerable<string> version2)
        {
            int result = 0;

            IEnumerator<string> a = version1.GetEnumerator();
            IEnumerator<string> b = version2.GetEnumerator();

            bool aExists = a.MoveNext();
            bool bExists = b.MoveNext();

            while (aExists || bExists)
            {
                if (!aExists && bExists)
                    return -1;

                if (aExists && !bExists)
                    return 1;

                // compare the labels
                result = CompareRelease(a.Current, b.Current);

                if (result != 0)
                    return result;

                aExists = a.MoveNext();
                bExists = b.MoveNext();
            }

            return result;
        }

        /// <summary>
        /// Release labels are compared as numbers if they are numeric, otherwise they will be compared
        /// as strings.
        /// </summary>
        private static int CompareRelease(string version1, string version2)
        {
            int version1Num = 0;
            int version2Num = 0;
            int result = 0;

            // check if the identifiers are numeric
            bool v1IsNumeric = int.TryParse(version1, out version1Num);
            bool v2IsNumeric = int.TryParse(version2, out version2Num);

            // if both are numeric compare them as numbers
            if (v1IsNumeric && v2IsNumeric)
            {
                result = version1Num.CompareTo(version2Num);
            }
            else if (v1IsNumeric || v2IsNumeric)
            {
                // numeric labels come before alpha labels
                if (v1IsNumeric)
                {
                    result = -1;
                }
                else
                {
                    result = 1;
                }
            }
            else
            {
                // Ignoring 2.0.0 case sensitive compare. Everything will be compared case insensitively as 2.0.1 specifies.
                result = StringComparer.OrdinalIgnoreCase.Compare(version1, version2);
            }

            return result;
        }
    }
}
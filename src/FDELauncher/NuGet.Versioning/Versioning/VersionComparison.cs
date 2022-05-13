// https://github.com/NuGetArchive/NuGet.Versioning/blob/release/src/Versioning/VersionComparison.cs

namespace NuGet.Versioning
{
    /// <summary>
    /// Version comparison modes.
    /// </summary>
    public enum VersionComparison
    {
        /// <summary>
        /// Semantic version 2.0.1-rc comparison with additional compares for extra NuGetVersion fields.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Compares only the version numbers.
        /// </summary>
        Version = 1,

        /// <summary>
        /// Include Version number and Release labels in the compare.
        /// </summary>
        VersionRelease = 2,

        /// <summary>
        /// Include all metadata during the compare.
        /// </summary>
        VersionReleaseMetadata = 3
    }
}
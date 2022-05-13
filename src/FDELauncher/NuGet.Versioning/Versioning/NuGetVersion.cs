// https://github.com/NuGetArchive/NuGet.Versioning/blob/release/src/Versioning/NuGetVersion.cs

using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Versioning
{
    /// <summary>
    /// A hybrid implementation of SemVer that supports semantic versioning as described at http://semver.org while not strictly enforcing it to 
    /// allow older 4-digit versioning schemes to continue working.
    /// </summary>
    public partial class NuGetVersion : SemanticVersion
    {
        private readonly string? _originalString;

        /// <summary>
        /// Creates a NuGetVersion using NuGetVersion.Parse(string)
        /// </summary>
        /// <param name="version">Version string</param>
        public NuGetVersion(string version)
            : this(Parse(version))
        {

        }

        /// <summary>
        /// Creates a NuGetVersion from an existing NuGetVersion
        /// </summary>
        public NuGetVersion(NuGetVersion version)
            : this(version.Version, version.ReleaseLabels, version.Metadata, version.ToString())
        {

        }

        /// <summary>
        /// Creates a NuGetVersion from a .NET Version
        /// </summary>
        /// <param name="version">Version numbers</param>
        /// <param name="releaseLabel">Prerelease label</param>
        /// <param name="metadata">Build metadata</param>
        public NuGetVersion(Version version, string? releaseLabel = null, string? metadata = null)
            : this(version, ParseReleaseLabels(releaseLabel), metadata, GetLegacyString(version, ParseReleaseLabels(releaseLabel), metadata))
        {

        }

        /// <summary>
        /// Creates a NuGetVersion X.Y.Z
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        public NuGetVersion(int major, int minor, int patch)
            : this(major, minor, patch, Enumerable.Empty<string>(), null)
        {

        }

        /// <summary>
        /// Creates a NuGetVersion X.Y.Z-alpha
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        /// <param name="releaseLabel">Prerelease label</param>
        public NuGetVersion(int major, int minor, int patch, string releaseLabel)
            : this(major, minor, patch, ParseReleaseLabels(releaseLabel), null)
        {

        }

        /// <summary>
        /// Creates a NuGetVersion X.Y.Z-alpha#build01
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        /// <param name="releaseLabel">Prerelease label</param>
        /// <param name="metadata">Build metadata</param>
        public NuGetVersion(int major, int minor, int patch, string? releaseLabel, string? metadata)
            : this(major, minor, patch, ParseReleaseLabels(releaseLabel), metadata)
        {

        }

        /// <summary>
        /// Creates a NuGetVersion X.Y.Z-alpha.1.2#build01
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        /// <param name="releaseLabels">Prerelease labels</param>
        /// <param name="metadata">Build metadata</param>
        public NuGetVersion(int major, int minor, int patch, IEnumerable<string>? releaseLabels, string? metadata)
            : this(new Version(major, minor, patch, 0), releaseLabels, metadata, null)
        {

        }

        /// <summary>
        /// Creates a NuGetVersion W.X.Y.Z
        /// </summary>
        /// <param name="major">W.x.y.z</param>
        /// <param name="minor">w.X.y.z</param>
        /// <param name="patch">w.x.Y.z</param>
        /// <param name="revision">w.x.y.Z</param>
        public NuGetVersion(int major, int minor, int patch, int revision)
            : this(major, minor, patch, revision, Enumerable.Empty<string>(), null)
        {

        }

        /// <summary>
        /// Creates a NuGetVersion W.X.Y.Z-alpha#build01
        /// </summary>
        /// <param name="major">W.x.y.z</param>
        /// <param name="minor">w.X.y.z</param>
        /// <param name="patch">w.x.Y.z</param>
        /// <param name="revision">w.x.y.Z</param>
        /// <param name="releaseLabel">Prerelease label</param>
        /// <param name="metadata">Build metadata</param>
        public NuGetVersion(int major, int minor, int patch, int revision, string releaseLabel, string metadata)
            : this(major, minor, patch, revision, ParseReleaseLabels(releaseLabel), metadata)
        {

        }

        /// <summary>
        /// Creates a NuGetVersion W.X.Y.Z-alpha.1#build01
        /// </summary>
        /// <param name="major">W.x.y.z</param>
        /// <param name="minor">w.X.y.z</param>
        /// <param name="patch">w.x.Y.z</param>
        /// <param name="revision">w.x.y.Z</param>
        /// <param name="releaseLabels">Prerelease labels</param>
        /// <param name="metadata">Build metadata</param>
        public NuGetVersion(int major, int minor, int patch, int revision, IEnumerable<string>? releaseLabels, string? metadata)
            : this(new Version(major, minor, patch, revision), releaseLabels, metadata, null)
        {

        }

        /// <summary>
        /// Creates a NuGetVersion from a .NET Version with additional release labels, build metadata, and a non-normalized version string.
        /// </summary>
        /// <param name="version">Version numbers</param>
        /// <param name="releaseLabels">prerelease labels</param>
        /// <param name="metadata">Build metadata</param>
        /// <param name="originalVersion">Non-normalized original version string</param>
        public NuGetVersion(Version version, IEnumerable<string>? releaseLabels, string? metadata, string? originalVersion)
            : base(version, releaseLabels, metadata)
        {
            _originalString = originalVersion;
        }

        /// <summary>
        /// Returns the version string.
        /// </summary>
        /// <remarks>This method includes legacy behavior. Use ToNormalizedString() instead.</remarks>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(_originalString))
            {
                return ToNormalizedString();
            }

            return _originalString!;
        }

        /// <summary>
        /// A System.Version representation of the version without metadata or release labels.
        /// </summary>
        public Version Version
        {
            get { return _version; }
        }

        /// <summary>
        /// True if the NuGetVersion is using legacy behavior.
        /// </summary>
        public virtual bool IsLegacyVersion
        {
            get { return Version.Revision > 0; }
        }

        /// <summary>
        /// Revision version R (x.y.z.R)
        /// </summary>
        public int Revision { get { return _version.Revision; } }
    }
}
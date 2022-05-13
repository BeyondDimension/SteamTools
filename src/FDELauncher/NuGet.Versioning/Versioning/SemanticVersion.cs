// https://github.com/NuGetArchive/NuGet.Versioning/blob/release/src/Versioning/SemanticVersion.cs

using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Versioning
{
    /// <summary>
    /// A strict SemVer implementation
    /// </summary>
    public partial class SemanticVersion : SimpleVersion
    {
        internal readonly IEnumerable<string>? _releaseLabels;
        internal readonly string? _metadata;
        internal Version _version;

        /// <summary>
        /// Creates a SemanticVersion from an existing SemanticVersion
        /// </summary>
        public SemanticVersion(SemanticVersion version)
            : this(version.Major, version.Minor, version.Patch, version.ReleaseLabels, version.Metadata)
        {

        }

        /// <summary>
        /// Creates a SemanticVersion X.Y.Z
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        public SemanticVersion(int major, int minor, int patch)
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
        public SemanticVersion(int major, int minor, int patch, string releaseLabel)
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
        public SemanticVersion(int major, int minor, int patch, string releaseLabel, string metadata)
            : this(major, minor, patch, ParseReleaseLabels(releaseLabel), metadata)
        {

        }

        /// <summary>
        /// Creates a NuGetVersion X.Y.Z-alpha.1.2#build01
        /// </summary>
        /// <param name="major">X.y.z</param>
        /// <param name="minor">x.Y.z</param>
        /// <param name="patch">x.y.Z</param>
        /// <param name="releaseLabels">Release labels that have been split by the dot separator</param>
        /// <param name="metadata">Build metadata</param>
        public SemanticVersion(int major, int minor, int patch, IEnumerable<string>? releaseLabels, string? metadata)
            : this(new Version(major, minor, patch, 0), releaseLabels, metadata)
        {

        }

        protected SemanticVersion(Version version, string? releaseLabel = null, string? metadata = null)
            : this(version, ParseReleaseLabels(releaseLabel), metadata)
        {

        }

        protected SemanticVersion(int major, int minor, int patch, int revision, string releaseLabel, string metadata)
            : this(major, minor, patch, revision, ParseReleaseLabels(releaseLabel), metadata)
        {

        }

        protected SemanticVersion(int major, int minor, int patch, int revision, IEnumerable<string>? releaseLabels, string metadata)
            : this(new Version(major, minor, patch, revision), releaseLabels, metadata)
        {

        }

        protected SemanticVersion(Version version, IEnumerable<string>? releaseLabels, string? metadata)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }

            _version = NormalizeVersionValue(version);
            _metadata = metadata;

            if (releaseLabels != null)
            {
                // enumerate the list
                _releaseLabels = releaseLabels.ToArray();
            }
        }

        /// <summary>
        /// Major version X (X.y.z)
        /// </summary>
        public int Major { get { return _version.Major; } }

        /// <summary>
        /// Minor version Y (x.Y.z)
        /// </summary>
        public int Minor { get { return _version.Minor; } }

        /// <summary>
        /// Patch version Z (x.y.Z)
        /// </summary>
        public int Patch { get { return _version.Build; } }

        /// <summary>
        /// A collection of pre-release labels attached to the version.
        /// </summary>
        public IEnumerable<string> ReleaseLabels
        {
            get
            {
                return _releaseLabels ?? Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// The full pre-release label for the version.
        /// </summary>
        public string Release
        {
            get
            {
                if (_releaseLabels != null)
                {
                    return string.Join(".", _releaseLabels.ToArray());
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// True if pre-release labels exist for the version.
        /// </summary>
        public virtual bool IsPrerelease
        {
            get
            {
                if (ReleaseLabels != null)
                {
                    var enumerator = ReleaseLabels.GetEnumerator();
                    return enumerator.MoveNext() && !string.IsNullOrEmpty(enumerator.Current);
                }

                return false;
            }
        }

        /// <summary>
        /// True if metadata exists for the version.
        /// </summary>
        public virtual bool HasMetadata
        {
            get
            {
                return !string.IsNullOrEmpty(Metadata);
            }
        }

        /// <summary>
        /// Build metadata attached to the version.
        /// </summary>
        public virtual string? Metadata
        {
            get
            {
                return _metadata;
            }
        }
    }
}
// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/BundleInfo/Bundle.cs

using Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo.Versioning;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo
{
    internal abstract class Bundle : IEquatable<Bundle>
    {
        public BundleVersion Version { get; }
        public BundleArch Arch { get; }
        public string UninstallCommand { get; }
        public string DisplayName { get; }

        public Bundle(BundleVersion version, BundleArch arch, string uninstallCommand, string displayName)
        {
            if (version == null || uninstallCommand == null || displayName == null)
            {
                throw new ArgumentNullException();
            }

            Version = version;
            Arch = arch;
            UninstallCommand = uninstallCommand;
            DisplayName = displayName;
        }

        public static Bundle From(BundleVersion version, BundleArch arch, string uninstallCommand, string displayName)
        {
            return version.ToBundle(arch, uninstallCommand, displayName);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Bundle);
        }

        public bool Equals(Bundle? other)
        {
            return other != null &&
                   EqualityComparer<BundleVersion>.Default.Equals(Version, other.Version) &&
                   Arch == other.Arch;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Version, Arch);
        }

        public override string ToString()
        {
            return $"{Version} ({Arch.ToString().ToLower()})";
        }
    }

    internal class Bundle<TBundleVersion> : Bundle, IComparable, IComparable<Bundle>, IEquatable<Bundle<TBundleVersion>>
        where TBundleVersion : BundleVersion, IComparable<TBundleVersion>
    {
        public new TBundleVersion Version => (base.Version as TBundleVersion)!;

        public Bundle(TBundleVersion version, BundleArch arch, string uninstallCommand, string displayName) :
            base(version, arch, uninstallCommand, displayName)
        { }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as Bundle);
        }

        public int CompareTo(Bundle? other)
        {
            if (other == null)
            {
                return 1;
            }

            return Version.Equals(other.Version) ?
                Arch - other.Arch :
                Version.SemVer.CompareTo(other.Version.SemVer);
        }

        public static IEnumerable<Bundle<TBundleVersion>> FilterWithSameBundleType(IEnumerable<Bundle> bundles)
        {
            return bundles
                .Where(bundle => bundle.Version is TBundleVersion)
                .Select(bundle => (bundle as Bundle<TBundleVersion>)!);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Bundle<TBundleVersion>);
        }

        public bool Equals(Bundle<TBundleVersion>? other)
        {
            return other != null &&
                   base.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode());
        }
    }
}
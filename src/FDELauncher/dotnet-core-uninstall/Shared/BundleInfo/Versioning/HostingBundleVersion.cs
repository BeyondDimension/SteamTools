// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/BundleInfo/Versioning/HostingBundleVersion.cs

namespace Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo.Versioning
{
    internal class HostingBundleVersion : BundleVersion, IComparable, IComparable<HostingBundleVersion>, IEquatable<HostingBundleVersion>
    {
        public override BundleType Type => BundleType.HostingBundle;
        public override BeforePatch BeforePatch => new MajorMinorVersion(Major, Minor);

        public HostingBundleVersion() { }

        public HostingBundleVersion(string value, string? footnote = null) : base(value, footnote) { }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as HostingBundleVersion);
        }

        public int CompareTo(HostingBundleVersion? other)
        {
            return other == null ? 1 : SemVer.CompareTo(other.SemVer);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HostingBundleVersion);
        }

        public bool Equals(HostingBundleVersion? other)
        {
            return other != null &&
                   base.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode());
        }

        public override Bundle ToBundle(BundleArch arch, string uninstallCommand, string displayName)
        {
            return new Bundle<HostingBundleVersion>(this, arch, uninstallCommand, displayName);
        }
    }
}
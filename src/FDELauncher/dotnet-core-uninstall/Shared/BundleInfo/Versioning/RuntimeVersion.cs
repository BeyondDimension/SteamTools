// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/BundleInfo/Versioning/RuntimeVersion.cs

namespace Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo.Versioning
{
    internal class RuntimeVersion : BundleVersion, IComparable, IComparable<RuntimeVersion>, IEquatable<RuntimeVersion>
    {
        public override BundleType Type => BundleType.Runtime;
        public override BeforePatch BeforePatch => new MajorMinorVersion(Major, Minor);

        public RuntimeVersion() { }

        public RuntimeVersion(string value) : base(value) { }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as RuntimeVersion);
        }

        public int CompareTo(RuntimeVersion? other)
        {
            return other == null ? 1 : SemVer.CompareTo(other.SemVer);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RuntimeVersion);
        }

        public bool Equals(RuntimeVersion? other)
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
            return new Bundle<RuntimeVersion>(this, arch, uninstallCommand, displayName);
        }
    }
}
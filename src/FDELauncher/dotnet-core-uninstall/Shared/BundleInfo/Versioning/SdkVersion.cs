// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/BundleInfo/Versioning/SdkVersion.cs

using System;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo.Versioning
{
    internal class SdkVersion : BundleVersion, IComparable, IComparable<SdkVersion>, IEquatable<SdkVersion>
    {
        public int SdkMinor => SemVer.Patch / 100;
        public override int Patch => SemVer.Patch % 100;
        public override BeforePatch BeforePatch => new MajorMinorSdkMinorVersion(Major, Minor, SdkMinor);

        public override BundleType Type => BundleType.Sdk;

        public SdkVersion() { }

        public SdkVersion(string value) : base(value) { }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as SdkVersion);
        }

        public int CompareTo(SdkVersion? other)
        {
            return other == null ? 1 : SemVer.CompareTo(other.SemVer);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SdkVersion);
        }

        public bool Equals(SdkVersion? other)
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
            return new Bundle<SdkVersion>(this, arch, uninstallCommand, displayName);
        }
    }
}
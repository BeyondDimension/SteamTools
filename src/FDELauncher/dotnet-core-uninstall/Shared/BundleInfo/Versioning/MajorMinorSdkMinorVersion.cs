// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/BundleInfo/Versioning/MajorMinorSdkMinorVersion.cs

using System;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo.Versioning
{
    internal class MajorMinorSdkMinorVersion : BeforePatch, IEquatable<MajorMinorSdkMinorVersion>, IComparable, IComparable<MajorMinorSdkMinorVersion>
    {
        public int SdkMinor { get; }

        public MajorMinorSdkMinorVersion(int major, int minor, int sdkMinor) : base(major, minor)
        {
            if (sdkMinor < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            SdkMinor = sdkMinor;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MajorMinorSdkMinorVersion);
        }

        public bool Equals(MajorMinorSdkMinorVersion? other)
        {
            return other != null &&
                   base.Equals(other) &&
                   SdkMinor == other.SdkMinor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), SdkMinor);
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as MajorMinorSdkMinorVersion);
        }

        public int CompareTo(MajorMinorSdkMinorVersion? other)
        {
            if (other == null)
            {
                return 1;
            }

            return base.Equals(other) ?
                SdkMinor.CompareTo(other.SdkMinor) :
                _version.CompareTo(other._version);
        }
    }
}
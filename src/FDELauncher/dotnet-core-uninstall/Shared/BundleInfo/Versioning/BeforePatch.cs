// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/BundleInfo/Versioning/BeforePatch.cs

using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo.Versioning
{
    internal abstract class BeforePatch : IEquatable<BeforePatch>
    {
        protected internal readonly Version _version;

        public int Major => _version.Major;
        public int Minor => _version.Minor;

        public BeforePatch(int major, int minor)
        {
            if (major < 0 || minor < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _version = new Version(major, minor);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BeforePatch);
        }

        public bool Equals(BeforePatch? other)
        {
            return other != null &&
                   EqualityComparer<Version>.Default.Equals(_version, other._version);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_version);
        }
    }
}
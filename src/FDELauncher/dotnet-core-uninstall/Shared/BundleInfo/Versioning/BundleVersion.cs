// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/BundleInfo/Versioning/BundleVersion.cs

using System;
using System.Collections.Generic;
using Microsoft.DotNet.Tools.Uninstall.Shared.Exceptions;
using NuGet.Versioning;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo.Versioning
{
    internal abstract class BundleVersion
    {
        public abstract BundleType Type { get; }
        public abstract BeforePatch BeforePatch { get; }
        public string? Footnote { get; private set; }
        public bool HasFootnote => Footnote != null;

        public SemanticVersion SemVer { get; private set; }

        public virtual int Major => SemVer.Major;
        public virtual int Minor => SemVer.Minor;
        public virtual int Patch => SemVer.Patch;
        public virtual bool IsPrerelease => SemVer.IsPrerelease;
        public virtual MajorMinorVersion MajorMinor => new(Major, Minor);

        protected BundleVersion()
        {
            SemVer = null!;
        }

        protected BundleVersion(string value, string? footnote = null)
        {
            if (SemanticVersion.TryParse(value, out var semVer))
            {
                SemVer = semVer;
            }
            else if (value != null && SemanticVersion.TryParse(value.Replace(" ", ""), out var formattedSemVer))
            {
                SemVer = formattedSemVer;
            }
            else
            {
                throw new InvalidInputVersionException(value!);
            }

            Footnote = footnote;
        }

        public static TBundleVersion FromInput<TBundleVersion>(string value, string? footnote = null)
            where TBundleVersion : BundleVersion, new()
        {
            if (SemanticVersion.TryParse(value, out var semVer))
            {
                return new TBundleVersion
                {
                    SemVer = semVer,
                    Footnote = footnote
                };
            }

            throw new InvalidInputVersionException(value);
        }

        public static bool TryFromInput<TBundleVersion>(string value, out TBundleVersion? version)
            where TBundleVersion : BundleVersion, new()
        {
            if (SemanticVersion.TryParse(value, out var semVer))
            {
                version = new TBundleVersion
                {
                    SemVer = semVer
                };
                return true;
            }

            version = null;
            return false;
        }

        public override string ToString()
        {
            return SemVer.ToString();
        }

        public string ToStringWithAsterisk()
        {
            var asterisk = HasFootnote ? " (*)" : "";
            return $"{SemVer}{asterisk}";
        }

        protected bool Equals(BundleVersion other)
        {
            return other != null &&
                   EqualityComparer<SemanticVersion>.Default.Equals(SemVer, other.SemVer);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SemVer);
        }

        public abstract Bundle ToBundle(BundleArch arch, string uninstallCommand, string displayName);

        public SemanticVersion GetVersionWithoutTags()
        {
            return new SemanticVersion(Major, Minor, SemVer.Patch);
        }
    }
}
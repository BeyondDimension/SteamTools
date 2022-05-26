// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Windows/RegistryQuery.cs

using System.Text.RegularExpressions;
using Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo;
using Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo.Versioning;
using Microsoft.DotNet.Tools.Uninstall.Shared.Utils;
using Microsoft.DotNet.Tools.Uninstall.Shared.VSVersioning;
using Microsoft.Win32;

namespace Microsoft.DotNet.Tools.Uninstall.Windows
{
    internal static class RegistryQuery
    {
        public static IEnumerable<Bundle> GetInstalledBundles()
        {
            return VisualStudioSafeVersionsExtractor.GetUninstallableBundles(GetAllInstalledBundles());
        }

        public static IEnumerable<Bundle> GetAllInstalledBundles()
        {
            var bundles = GetNetCoreBundleKeys(RegistryKey2.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64));
            bundles = bundles.Concat(GetNetCoreBundleKeys(RegistryKey2.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)));

            var wrappedBundles = bundles
              .Select(WrapRegistryKey)
              .Where(bundle => bundle != null)
              .ToArray();

            return wrappedBundles!;
        }

        private static IEnumerable<RegistryKey> GetNetCoreBundleKeys(RegistryKey uninstallKey)
        {
            try
            {
                var uninstalls = uninstallKey
                    .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

                var names = uninstalls.GetSubKeyNames();

                return names
                    .Select(name => uninstalls.OpenSubKey(name))
                    .Where(IsNetCoreBundle);
            }
            catch
            {
                return Enumerable.Empty<RegistryKey>();
            }

        }

        private static bool IsNetCoreBundle(RegistryKey uninstallKey)
        {
            if (uninstallKey == null)
            {
                return false;
            }

            return IsNetCoreBundle(uninstallKey.GetValue("DisplayName")?.ToString() ?? "",
                uninstallKey.GetValue("DisplayVersion")?.ToString() ?? "",
                uninstallKey.GetValue("UninstallString")?.ToString() ?? "",
                uninstallKey.GetValue("BundleVersion")?.ToString() ?? "");
        }

        internal static bool IsNetCoreBundle(string displayName, string displayVersion, string uninstallString, string bundleVersion)
        {
            return (!string.IsNullOrEmpty(displayName)) &&
                    (displayName.IndexOf("Visual Studio", StringComparison.OrdinalIgnoreCase) < 0) &&
                    (displayName.IndexOf("VS 2015", StringComparison.OrdinalIgnoreCase) < 0) &&
                    (displayName.IndexOf("Local Feed", StringComparison.OrdinalIgnoreCase) < 0) &&
                    ((displayName.IndexOf(".NET", StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (displayName.IndexOf(".NET Runtime", StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (displayName.IndexOf(".NET SDK", StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (displayName.IndexOf("Dotnet Shared Framework for Windows Desktop", StringComparison.OrdinalIgnoreCase) >= 0) ||
                     (displayName.IndexOf("Windows Desktop Runtime", StringComparison.OrdinalIgnoreCase) >= 0)) &&
                    (!string.IsNullOrEmpty(uninstallString)) &&
                    (uninstallString.IndexOf(".exe", StringComparison.OrdinalIgnoreCase) >= 0) &&
                    (uninstallString.IndexOf("msiexec", StringComparison.OrdinalIgnoreCase) < 0) &&
                    (!string.IsNullOrEmpty(displayVersion)) &&
                    (!string.IsNullOrEmpty(bundleVersion));
        }

        private static Bundle? WrapRegistryKey(RegistryKey registryKey)
        {
            var displayName = registryKey.GetValue("DisplayName")?.ToString() ?? "";
            var uninstallCommand = registryKey.GetValue("QuietUninstallString")?.ToString() ?? registryKey.GetValue("UninstallString")?.ToString() ?? "";
            var bundleCachePath = registryKey.GetValue("BundleCachePath")?.ToString() ?? "";

            var version = GetBundleVersion(displayName, uninstallCommand, bundleCachePath);
            var arch = GetBundleArch(displayName, bundleCachePath);

            if (version == null)
            {
                return null;
            }

            return Bundle.From(version, arch, uninstallCommand, displayName);
        }

        public static BundleVersion? GetBundleVersion(string displayName, string uninstallString, string bundleCachePath)
        {
            var versionString = Regexes.VersionDisplayNameRegex.Match(displayName)?.Value ?? string.Empty;
            string? footnote = null;
            if (bundleCachePath != null)
            {
                var cachePathMatch = Regexes.BundleCachePathRegex.Match(bundleCachePath);
                var hasAuxVersion = cachePathMatch.Groups[Regexes.AuxVersionGroupName].Success;
                footnote = hasAuxVersion ?
                    string.Format(LocalizableStrings.HostingBundleFootnoteFormat, displayName, versionString) :
                    null;
            }

            try
            {
                // Classify the bundle type
                if (displayName.IndexOf("Windows Server", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return new HostingBundleVersion(versionString, footnote);
                }
                else if (displayName.IndexOf("ASP.NET", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return new AspNetRuntimeVersion(versionString);
                }
                else if ((displayName.IndexOf(".NET Core SDK", StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (displayName.IndexOf("Microsoft .NET SDK", StringComparison.OrdinalIgnoreCase) >= 0) ||
                        uninstallString.IndexOf("dotnet-dev-win") >= 0)
                {
                    return new SdkVersion(versionString);
                }
                else if (displayName.IndexOf(".NET Core Runtime", StringComparison.OrdinalIgnoreCase) >= 0 || Regex.IsMatch(displayName, @".*\.NET Core.*Runtime") ||
                    displayName.IndexOf(".NET Runtime", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return new RuntimeVersion(versionString);
                }
                else if (displayName.IndexOf("Windows Desktop Runtime", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return new WindowsDesktopRuntimeVersion(versionString);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private static BundleArch GetBundleArch(string displayName, string bundleCachePath)
        {
            const string x64String = "x64";
            const string x86String = "x86";
            const string arm64String = "arm64";

            string? archString = null;
            if (bundleCachePath != null)
            {
                var cachePathMatch = Regexes.BundleCachePathRegex.Match(bundleCachePath);
                archString = cachePathMatch.Groups[Regexes.ArchGroupName].Value;
            }

            if (string.IsNullOrEmpty(archString))
            {
                archString = displayName.Contains(x64String) ?
                    x64String :
                    displayName.Contains(x86String) ? x86String : string.Empty;

                archString = archString switch
                {
                    string a when a.Contains(x64String) => x64String,
                    string b when b.Contains(x86String) => x86String,
                    string b when b.Contains(arm64String) => arm64String,
                    _ => string.Empty
                };
            }

            return archString switch
            {
                x64String => BundleArch.X64,
                x86String => BundleArch.X86,
                arm64String => BundleArch.Arm64,
                "" => BundleArch.X64 | BundleArch.X86,
                _ => throw new ArgumentException(),
            };
        }
    }
}
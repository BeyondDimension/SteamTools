// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/Utils/Regexes.cs

using System.Text.RegularExpressions;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.Utils
{
    internal static class Regexes
    {
        public static readonly string MajorGroupName = "major";
        public static readonly string MinorGroupName = "minor";
        public static readonly string SdkMinorGroupName = "sdkMinor";
        public static readonly string PatchGroupName = "patch";
        public static readonly string PreviewGroupName = "preview";
        public static readonly string BuildGroupName = "build";
        public static readonly string TypeGroupName = "type";
        public static readonly string ArchGroupName = "arch";
        public static readonly string VersionGroupName = "version";
        public static readonly string AuxVersionGroupName = "auxVersion";

        private static readonly Regex _majorMinorRegex = new(
            $@"(?<{MajorGroupName}>\d+)\.(?<{MinorGroupName}>\d+)");
        private static readonly Regex _previewVersionNumberRegex = new(
            $@"(\d+(\.\d+)*)?");
        private static readonly Regex _rcVersionNumberRegex = new(
            $@"\d+");
        private static readonly Regex _buildNumberRegex = new(
            $@"(?<{BuildGroupName}>\d+)");
        private static readonly Regex _archRegex = new(
            $@"(?<{ArchGroupName}>\-?x64|x86)");

        private static readonly Regex _previewVersionSdkDisplayNameRegex = new(
            $@"(?<{PreviewGroupName}>\s?\-\s?((preview|alpha)\.?{_previewVersionNumberRegex}|rc{_rcVersionNumberRegex}))");
        private static readonly Regex _previewVersionSdkCachePathRegex = new(
            $@"(?<{PreviewGroupName}>\-((preview|alpha)\.?{_previewVersionNumberRegex}|rc{_rcVersionNumberRegex}(\.\d+)?)\-(?<{BuildGroupName}>\d+))");
        private static readonly Regex _previewVersionRuntimeCachePathRegex = new(
            $@"(?<{PreviewGroupName}>\-(preview{_previewVersionNumberRegex}\-{_buildNumberRegex}\-\d+|rc{_rcVersionNumberRegex}))");
        private static readonly Regex _previewVersionAspNetRuntimeCachePathRegex = new(
            $@"(?<{PreviewGroupName}>\-(preview{_previewVersionNumberRegex}(\.{_buildNumberRegex}\.\d+|\-(final|{_buildNumberRegex}(\-\d+)?))|rc{_rcVersionNumberRegex}\-final))");
        private static readonly Regex _previewVersionHostingBundleCachePathRegex = new(
            $@"(?<{PreviewGroupName}>\-(preview{_previewVersionNumberRegex}(\.{_buildNumberRegex}\.\d+|\-(final|{_buildNumberRegex}(\-\d+)?))|rc{_rcVersionNumberRegex}\-final))");

        private static readonly string _sdkVersionBasicRegexFormat =
            $@"(?<{VersionGroupName}>{_majorMinorRegex}\.((?<{SdkMinorGroupName}>\d+)(?<{PatchGroupName}>\d{{{{2}}}})|(?<{PatchGroupName}>\d{{{{1,2}}}}))({{0}})?)";
        private static readonly string _notCapturedRuntimeVersionBasicRegexFormat =
            $@"{_majorMinorRegex}\.(?<{PatchGroupName}>\d+)({{0}})?";
        private static readonly string _runtimeVersionBasicRegexFormat =
            $@"(?<{VersionGroupName}>{_notCapturedRuntimeVersionBasicRegexFormat})";
        private static readonly string _runtimeAuxVersionBasicRegexFormat =
            $@"(?<{AuxVersionGroupName}>{_notCapturedRuntimeVersionBasicRegexFormat})";
        private static readonly Regex _sdkVersionCachePathRegex = new(string.Format(
            _sdkVersionBasicRegexFormat,
            _previewVersionSdkCachePathRegex.ToString()));
        private static readonly Regex _runtimeVersionCachePathRegex = new(string.Format(
            _runtimeVersionBasicRegexFormat,
            _previewVersionRuntimeCachePathRegex.ToString()));
        private static readonly Regex _aspNetRuntimeVersionCachePathRegex = new(string.Format(
            _runtimeVersionBasicRegexFormat,
            _previewVersionAspNetRuntimeCachePathRegex.ToString()));
        private static readonly Regex _hostingBundleVersionCachePathRegex = new(string.Format(
            _runtimeVersionBasicRegexFormat,
            _previewVersionHostingBundleCachePathRegex.ToString()));
        private static readonly Regex _hostingBundleAuxVersionCachePathRegex = new(string.Format(
            _runtimeAuxVersionBasicRegexFormat,
            _previewVersionHostingBundleCachePathRegex.ToString()));
        private static readonly Regex _sdkCachePathRegex = new(
            $@"\\dotnet\-(?<{TypeGroupName}>sdk)\-{_sdkVersionCachePathRegex}\-win\-{_archRegex}\.exe|\\dotnet\-dev\-win\-{_archRegex}\.{_sdkVersionCachePathRegex}\.exe");
        private static readonly Regex _runtimeCachePathRegex = new(
            $@"\\dotnet\-(?<{TypeGroupName}>runtime)\-{_runtimeVersionCachePathRegex}\-win\-{_archRegex}\.exe|\\dotnet\-win\-{_archRegex}\.{_runtimeVersionCachePathRegex}\.exe");
        private static readonly Regex _aspNetRuntimeCachePathRegex = new(
            $@"\\(?<{TypeGroupName}>AspNetCore)\.{_aspNetRuntimeVersionCachePathRegex}\.RuntimePackageStore_{_archRegex}\.exe|\\(?<{TypeGroupName}>aspnetcore\-runtime)\-{_aspNetRuntimeVersionCachePathRegex}\-win\-{_archRegex}\.exe");
        private static readonly Regex _aspNetSharedFrameworkCachePathRegex = new(
            $@"\\(?<{TypeGroupName}>AspNetCoreSharedFrameworkBundle)-{_archRegex}\.exe");
        private static readonly Regex _hostingBundleCachePathRegex = new(
            $@"\\DotNetCore\.({_hostingBundleAuxVersionCachePathRegex}_)?{_hostingBundleVersionCachePathRegex}\-(?<{TypeGroupName}>WindowsHosting)\.exe|\\dotnetcore\.{_hostingBundleAuxVersionCachePathRegex}_{_hostingBundleVersionCachePathRegex}\-(?<{TypeGroupName}>windowshosting)\.exe|\\dotnet\-(?<{TypeGroupName}>hosting)\-{_hostingBundleVersionCachePathRegex}\-win\.exe");

        public static readonly Regex VersionDisplayNameRegex = new(string.Format(
            _sdkVersionBasicRegexFormat,
            _previewVersionSdkDisplayNameRegex.ToString()));
        public static readonly Regex BundleMajorMinorRegex = new(
            $@"^{_majorMinorRegex}$");
        public static readonly Regex BundleCachePathRegex = new(
            $@"({_sdkCachePathRegex}|{_runtimeCachePathRegex}|{_aspNetRuntimeCachePathRegex}|{_aspNetSharedFrameworkCachePathRegex}|{_hostingBundleCachePathRegex})$");
    }
}
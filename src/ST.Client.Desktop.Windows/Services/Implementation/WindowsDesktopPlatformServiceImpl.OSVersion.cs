using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace System.Application.Services.Implementation
{
    partial class WindowsDesktopPlatformServiceImpl
    {
        static (string productName, int revision, string releaseIdOrDisplayVersion) GetWinNTCurrentVersion()
        {
            const string subkey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            using var ndpKey = Registry.LocalMachine.OpenSubKey(subkey);
            var productName = ndpKey?.GetValue("ProductName")?.ToString();
            var revision = ndpKey?.GetValue("UBR")?.ToString();
            var revisionInt32 = 0;
            if (!int.TryParse(revision, out revisionInt32))
            {
                var path_kernel32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                    "kernel32.dll");
                if (File.Exists(path_kernel32))
                {
                    revisionInt32 = FileVersionInfo.GetVersionInfo(path_kernel32).ProductPrivatePart;
                }
            }
            var releaseId = ndpKey?.GetValue("ReleaseId")?.ToString();
            if (int.TryParse(releaseId, out var releaseIdInt32) && releaseIdInt32 >= 2009)
            {
                var displayVersion = ndpKey?.GetValue("DisplayVersion")?.ToString();
                if (!string.IsNullOrWhiteSpace(displayVersion))
                    releaseId = displayVersion;
            }
            return (productName, revisionInt32, releaseId);
        }

        static readonly Lazy<(string productName, int revision, string releaseIdOrDisplayVersion)> _WinNTCurrentVersion = new(GetWinNTCurrentVersion);

        public string WindowsProductName => _WinNTCurrentVersion.Value.productName;

        public int WindowsVersionRevision => _WinNTCurrentVersion.Value.revision;

        public string WindowsReleaseIdOrDisplayVersion => _WinNTCurrentVersion.Value.releaseIdOrDisplayVersion;
    }
}

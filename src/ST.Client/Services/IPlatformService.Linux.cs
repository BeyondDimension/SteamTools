using System.Diagnostics;
using System.Runtime.Versioning;
using System.IO;
using System.Linq;

namespace System.Application.Services
{
    partial interface IPlatformService
    {
        static readonly Lazy<string> _LinuxIssue = new(() =>
        {
            if (OperatingSystem2.IsLinux)
            {
                var filePath = string.Format("{0}etc{0}issue", Path.DirectorySeparatorChar);
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath).TrimEnd(" \\n \\l\n\n");
                }
            }
            return string.Empty;
        });

        /// <summary>
        /// 获取当前 Linux 系统发行版名称
        /// </summary>
        [SupportedOSPlatform("Linux")]
        string LinuxIssue => _LinuxIssue.Value;

        /// <summary>
        /// 获取当前 Linux 系统发行版是否为 深度操作系统（deepin）
        /// </summary>
        [SupportedOSPlatform("Linux")]
        bool IsDeepin => LinuxIssue.Contains("Deepin", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 获取当前 Linux 系统发行版是否为 Ubuntu
        /// </summary>
        [SupportedOSPlatform("Linux")]
        bool IsUbuntu => LinuxIssue.Contains("Ubuntu", StringComparison.OrdinalIgnoreCase);
    }
}

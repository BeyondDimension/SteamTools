/* 适用于 Windwos 7 OS 的 疑难解答助手
 * 主要逻辑：if 安装了补丁 KB3063858
 * 直接运行主程序
 * else 本地化文本提示未安装补丁，引导用户进行下载
 * 例如：输入 y 跳转微软官方下载中心下载补丁安装后再使用本应用程序，输入 n 退出
 * Win7 自带 net35 可能魔改版删除了此功能，那就没办法了
 * 可在 控制面板\程序\程序和功能 - 打开或关闭 Windows 功能 - Microsoft .NET Framework 3.5.1 根据左边的勾选框，非空心即代表已安装
 * 对于本程序
 * 在配置文件 exe.config supportedRuntime 项中可添加net4x的支持
 */

using Microsoft.Win32;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Properties;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace System
{
    static class Program
    {
        internal static Mutex? mutex;
        internal static readonly Process currentProcess = Process.GetCurrentProcess();
        internal static string thisFilePath = "";
        internal static string appFilePath = "";
        internal const string binDirName = "bin";
        internal const string win7MarkName = ".win7";

        /// <summary>
        /// 在浏览器中打开补丁KB3063858的下载地址
        /// </summary>
        internal static void Open_KB3063858_DownloadLink()
        {
            var downloadLink = EnvironmentEx.Is64BitOperatingSystem ?
                DownloadLinks.KB3063858_x64 : DownloadLinks.KB3063858_x86;
            downloadLink = string.Format(downloadLink, CultureInfo.CurrentCulture.Name);
            Process.Start(downloadLink);
        }

        /// <summary>
        /// 检查是否安装了补丁KB3063858
        /// </summary>
        /// <returns></returns>
        internal static bool CheckInstalled_KB3063858()
        {
            using var p = new Process();
            p.StartInfo.FileName = "cmd";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine("wmic qfe&exit");
            p.StandardInput.AutoFlush = true;
            var str = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            var items = str.Split(new[] { "KB" }, StringSplitOptions.RemoveEmptyEntries).Where(x => x.Length >= 7).Select(x => x.Substring(0, 7));
            foreach (var item in items)
            {
                if (int.TryParse(item, out var value))
                {
                    if (value >= 3063858) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 启动主APP
        /// </summary>
        internal static void AppRun()
        {
            if (string.IsNullOrEmpty(appFilePath) || !File.Exists(appFilePath)) return;
            Process.Start(appFilePath);
        }

        /// <summary>
        /// 返回结果：
        /// <para><see langword="true"/> 当前运行的操作系统为 Win7SP1</para>
        /// <para><see langword="false"/> 当前运行的操作系统不支持 </para>
        /// <para><see langword="null"/> 当前运行的操作系统为Win8或更高</para>
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        internal static bool? IsWin7SP1OrNotSupportedPlatform([NotNullWhen(false)] out string? error)
        {
            error = null;
            if (Environment.OSVersion.Version.Major == 6)
            {
                if (Environment.OSVersion.Version.Minor == 1) // NT 6.1 / Win7 / WinServer 2008 R2
                {
                    return Environment.OSVersion.ServicePack == "Service Pack 1";
                }
                else if (Environment.OSVersion.Version.Minor == 2) // NT 6.2 / Win8 / WinServer 2012
                {
                    error = SR.NotSupportedWin8PlatformError;
                    return false;
                }
                else if (Environment.OSVersion.Version.Minor == 3) // NT 6.3 / Win8.1 / WinServer 2012 R2
                {
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }
            else if (Environment.OSVersion.Version.Major < 6)
            {
                error = SR.NotSupportedPlatformError;
                return false;
            }
            return null;
        }

        #region QuickStart

        /* 快速启动功能
         * MSI安装包生成的桌面快捷方式指向当前程序
         * 在Win7上进行KB补丁检查或其他环境检查需要一定耗时
         * 预期仅第一次运行检查，检测通过后后续运行不在检查
         * 使用[MachineGuid/计算机名+用户名+系统版本号]进行哈希计算得到的值作为标识
         * 这样就算整个程序文件夹Copy另一台PC中，也会在第一次运行时重新检查
         */

        const string QuickStartMarkFileName = "quick_start_id.txt";

        internal static bool QuickStart(out Action? write)
        {
            try
            {
                var userDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var allDesktopPath = GetAllUsersDesktopFolderPath();

                var desktopPaths = new[] { userDesktopPath, allDesktopPath };

                if (desktopPaths.All(x => !File.Exists(Path.Combine(x, Path.GetFileNameWithoutExtension(appFilePath) + ".lnk"))))
                {
                    throw new Exception("Shortcut does not exist.");
                }

                var hashValue = Hashs.String.SHA256(GetUniqueIdentifier());
                static string GetUniqueIdentifier()
                {
                    var value = Registry.LocalMachine.Read(@"SOFTWARE\Microsoft\Cryptography", "MachineGuid");
                    var osVersion_ = Environment.OSVersion.Version;
                    var osVersion = osVersion_.Major + osVersion_.Minor.ToString();
                    if (value.IsNullOrWhiteSpace())
                    {
                        var machineName = Environment.MachineName;
                        var userName = Environment.UserName;
                        value = machineName + userName;
                    }
                    value += osVersion;
                    return value;
                }

                var path = Path.Combine(IOPath.AppDataDirectory, QuickStartMarkFileName);

                write = () =>
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            try
                            {
                                File.Delete(path);
                            }
                            catch
                            {
                            }
                        }
                        if (!File.Exists(path))
                        {
                            File.WriteAllText(path, hashValue);
                        }
                    }
                    catch
                    {
                    }
                };

                if (File.Exists(path))
                {
                    var hashValueByRead = File.ReadAllText(path);
                    if (hashValueByRead == hashValue)
                    {
                        return true;
                    }
                    else
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
                write = null;
            }

            return false;
        }

        [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
        static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

        const int MAX_PATH = 260;
        const int CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019;

        static string GetAllUsersDesktopFolderPath()
        {
            var @string = new StringBuilder(MAX_PATH);
            SHGetFolderPath(IntPtr.Zero, CSIDL_COMMON_DESKTOPDIRECTORY, IntPtr.Zero, 0, @string);
            return @string.ToString();
        }

        #endregion
    }
}
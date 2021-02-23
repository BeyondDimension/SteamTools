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

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Win7Troubleshoot.Properties;

namespace Win7Troubleshoot
{
    static class Program
    {
        internal static Mutex? mutex;

        [STAThread]
#pragma warning disable IDE0060 // 删除未使用的参数
        static void Main(string[] args)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException();
            mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out var isNotRunning);
            if (isNotRunning)
            {
                try
                {
                    var r = IsWin7SP1OrNotSupportedPlatform();
                    if (r.HasValue)
                    {
                        if (r.Value) // Win7SP1
                        {
                            if (!CheckInstalled_KB3063858())
                            {
                                if (MessageBox.Show(
                                    SR.Not_Installed_KB3063858,
                                    SR.Error,
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Error) == DialogResult.Yes)
                                {
                                    Open_KB3063858_DownloadLink();
                                }
                                return;
                            }
                        }
                        else // NotSupportedPlatform - For example, WinXP/Win2000
                        {
                            MessageBox.Show(
                                SR.NotSupportedPlatformError,
                                SR.Error,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }
                    }
                    AppRun();
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        e.ToString(),
                        SR.Error,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 在浏览器中打开补丁KB3063858的下载地址
        /// </summary>
        static void Open_KB3063858_DownloadLink()
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
        static bool CheckInstalled_KB3063858()
        {
            const string KB3063858 = "KB3063858";
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
            return str.Contains(KB3063858);
        }

        /// <summary>
        /// 启动主APP
        /// </summary>
        static void AppRun()
        {
            var fileName = Process.GetCurrentProcess().MainModule.FileName;
            fileName = fileName.Replace(".win7", string.Empty, StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName)) return;
            Process.Start(fileName);
        }

        /// <summary>
        /// 返回结果：
        /// <para><see langword="true"/> 当前运行的操作系统为 Win7SP1</para>
        /// <para><see langword="false"/> 当前运行的操作系统不支持 </para>
        /// <para><see langword="null"/> 当前运行的操作系统为Win8或更高</para>
        /// </summary>
        /// <returns></returns>
        static bool? IsWin7SP1OrNotSupportedPlatform()
        {
            if (Environment.OSVersion.Version.Major == 6)
            {
                if (Environment.OSVersion.Version.Minor == 1) // NT 6.1 / Win7 / WinServer 2008 R2
                {
                    return Environment.OSVersion.ServicePack == "Service Pack 1";
                }
                else if (Environment.OSVersion.Version.Minor == 2) // NT 6.2 / Win8 / WinServer 2012
                {
                    return false;
                }
                else if (Environment.OSVersion.Version.Minor == 3) // NT 6.3 / Win8.1 / WinServer 2012 R2
                {

                }
                else
                {
                    return false;
                }
            }
            else if (Environment.OSVersion.Version.Major < 6)
            {
                return false;
            }
            return null;
        }
    }
}
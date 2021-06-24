using System.Application.Models;
using System.Application.Models.Settings;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Windows;

namespace System.Application.Services
{
    public interface IDesktopPlatformService : IPlatformService
    {
        protected new const string TAG = "DesktopPlatformS";

        public new static IDesktopPlatformService Instance => DI.Get<IDesktopPlatformService>();

        void SetResizeMode(IntPtr hWnd, ResizeModeCompat value);

        /// <summary>
        /// 获取一个正在运行的进程的命令行参数。
        /// 与 <see cref="Environment.GetCommandLineArgs"/> 一样，使用此方法获取的参数是包含应用程序路径的。
        /// 关于 <see cref="Environment.GetCommandLineArgs"/> 可参见：
        /// .NET 命令行参数包含应用程序路径吗？https://blog.walterlv.com/post/when-will-the-command-line-args-contain-the-executable-path.html
        /// </summary>
        /// <param name="process">一个正在运行的进程。</param>
        /// <returns>表示应用程序运行命令行参数的字符串。</returns>
        string GetCommandLineArgs(Process process);

        /// <summary>
        /// hosts 文件所在目录
        /// </summary>
        string HostsFilePath => "/etc/hosts";

        /// <summary>
        /// 启动进程，传递文件名参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ComponentModel.Win32Exception"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IO.FileNotFoundException"></exception>
        void StartProcess(string name, string filePath) => Process.Start(name, $"\"{filePath}\"");

        void OpenProcess(string name, string arguments) => Process.Start(name, arguments);

        void OpenProcess(string name) => Process.Start(name);
        /// <summary>
        /// 使用文本阅读器打开文件
        /// </summary>
        /// <param name="filePath"></param>
        void OpenFileByTextReader(string filePath)
        {
            TextReaderProvider? userProvider = null;
            var p = GeneralSettings.TextReaderProvider.Value;
            if (p != null)
            {
                var platform = DI.Platform;
                if (p.ContainsKey(platform))
                {
                    var value = p[platform];
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        if (Enum.TryParse<TextReaderProvider>(value, out var enumValue))
                        {
                            userProvider = enumValue;
                        }
                        else
                        {
                            try
                            {
                                StartProcess(value, filePath);
                                return;
                            }
                            catch (Exception e)
                            {
                                Log.Error(TAG, e, "UserSettings OpenFileByTextReader Fail.");
                            }
                        }
                    }
                }
            }

            var providers = new List<TextReaderProvider>() {
                TextReaderProvider.Notepad,
                TextReaderProvider.VSCode };

            if (userProvider.HasValue)
            {
                providers.Remove(userProvider.Value);
                providers.Insert(0, userProvider.Value);
            }

            foreach (var item in providers)
            {
                try
                {
                    var fileName = GetFileName(item);
                    if (fileName == null) continue;
                    StartProcess(fileName, filePath);
                    return;
                }
                catch (Exception e)
                {
                    if (item == TextReaderProvider.Notepad)
                    {
                        Log.Error(TAG, e, "OpenFileByTextReader Fail.");
                    }
                }
            }
        }

        /// <summary>
        /// 使用资源管理器打开某个路径
        /// </summary>
        /// <param name="dirPath"></param>
        void OpenFolder(string dirPath);

        /// <summary>
        /// 设置系统关闭时任务
        /// </summary>
        void SetSystemSessionEnding(Action action);

        /// <summary>
        /// 获取文本阅读器提供商程序文件路径或文件名(如果提供程序已注册环境变量)
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        string? GetFileName(TextReaderProvider provider);

        /// <summary>
        /// 设置开机自启动
        /// </summary>
        /// <param name="isAutoStart">开启<see langword="true"/>、关闭<see langword="false"/></param>
        /// <param name="name"></param>
        void SetBootAutoStart(bool isAutoStart, string name);

        #region Steam

        /// <inheritdoc cref="ISteamService.SteamDirPath"/>
        string? GetSteamDirPath();

        /// <inheritdoc cref="ISteamService.SteamProgramPath"/>
        string? GetSteamProgramPath();

        /// <inheritdoc cref="ISteamService.GetLastLoginUserName"/>
        string GetLastSteamLoginUserName();

        /// <inheritdoc cref="ISteamService.SetCurrentUser(string)"/>
        void SetCurrentUser(string userName);

        #endregion

        #region MachineUniqueIdentifier

        private static (byte[] key, byte[] iv) GetMachineSecretKey(string? value)
        {
            value ??= string.Empty;
            var result = AESUtils.GetParameters(value);
            return result;
        }

        protected static Lazy<(byte[] key, byte[] iv)> GetMachineSecretKey(Func<string?> action) => new(() =>
        {
            string? value = null;
            try
            {
                value = action();
            }
            catch (Exception e)
            {
                Log.Warn(TAG, e, "GetMachineSecretKey Fail.");
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                value = Environment.MachineName;
            }
            return GetMachineSecretKey(value);
        });

        (byte[] key, byte[] iv) MachineSecretKey { get; }

        #endregion

        /// <summary>
        /// 是否应使用 亮色主题 <see langword="true"/> / 暗色主题 <see langword="false"/>
        /// </summary>
        bool? IsLightOrDarkTheme { get; }

        /// <summary>
        /// 设置亮色或暗色主题跟随系统
        /// </summary>
        /// <param name="enable"></param>
        void SetLightOrDarkThemeFollowingSystem(bool enable);

        /// <summary>
        /// 打开桌面图标设置
        /// </summary>
        [SupportedOSPlatform("Windows")]
        void OpenDesktopIconsSettings();

        [SupportedOSPlatform("Windows")]
        void OpenGameControllers();

        /// <summary>
        /// 已正常权限启动进程
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [SupportedOSPlatform("Windows")]
        Process StartAsInvoker(string fileName);

        /// <summary>
        /// 获取占用端口的进程
        /// </summary>
        /// <param name="port"></param>
        /// <param name="isTCPorUDP"></param>
        /// <returns></returns>
        [SupportedOSPlatform("Windows")]
        Process? GetProcessByPortOccupy(ushort port, bool isTCPorUDP = true);

        [SupportedOSPlatform("Windows")]
        bool IsAdministrator { get; }

        /// <summary>
        /// 从管理员权限进程中降权到普通权限启动进程
        /// </summary>
        /// <param name="cmdArgs"></param>
        [SupportedOSPlatform("Windows")]
        void UnelevatedProcessStart(string cmdArgs);

        void FixFluentWindowStyleOnWin7(IntPtr hWnd)
        {

        }
    }
}
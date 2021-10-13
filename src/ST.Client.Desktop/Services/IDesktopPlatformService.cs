using System.Application.Models;
using System.Application.Settings;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace System.Application.Services
{
    public partial interface IDesktopPlatformService : IPlatformService
    {
        protected new const string TAG = "DesktopPlatformS";

        public new static IDesktopPlatformService Instance => DI.Get<IDesktopPlatformService>();

        void SetResizeMode(IntPtr hWnd, ResizeMode value);

        /// <summary>
        /// hosts 文件所在目录
        /// </summary>
        string HostsFilePath => string.Format("{0}etc{0}hosts", Path.DirectorySeparatorChar);

        void IPlatformService.OpenFileByTextReader(string filePath)
        {
            TextReaderProvider? userProvider = null;
            var p = GeneralSettings.TextReaderProvider.Value;
            if (p != null)
            {
                var platform = DeviceInfo2.Platform;
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
                                Process2.StartPath(value, filePath);
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
                TextReaderProvider.VSCode,
                TextReaderProvider.Notepad };

            if (userProvider.HasValue)
            {
                providers.Remove(userProvider.Value);
                providers.Insert(0, userProvider.Value);
            }

            foreach (var item in providers)
            {
                if (item == TextReaderProvider.VSCode && !OperatingSystem2.IsWindows)
                {
                    // 其他平台的 VSCode 打开方式尚未实现
                    continue;
                }
                try
                {
                    var fileName = GetFileName(item);
                    if (fileName == null) continue;
                    Process2.StartPath(fileName, filePath);
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
        /// <param name="path"></param>
        void OpenFolder(string path)
        {
            sbyte isDirectory = -1;
            FileInfo? fileInfo;
            DirectoryInfo directoryInfo = new(path);
            if (directoryInfo.Exists) // 路径为文件夹，则打开文件夹
            {
                fileInfo = null;
                isDirectory = 1;
            }
            else
            {
                fileInfo = new(path);
                if (fileInfo.Exists) // 路径为文件，则打开文件
                {
                    isDirectory = 0;
                }
                else if (fileInfo.DirectoryName != null)
                {
                    directoryInfo = new(fileInfo.DirectoryName);
                    fileInfo = null;
                    if (directoryInfo.Exists) // 路径为文件但文件不存在，文件夹存在，则打开文件夹
                    {
                        fileInfo = null;
                        isDirectory = 1;
                    }
                }
            }

            switch (isDirectory)
            {
                case 1:
                    if (directoryInfo != null) OpenFolderByDirectoryPath(directoryInfo);
                    break;
                case 0:
                    if (fileInfo != null) OpenFolderSelectFilePath(fileInfo);
                    break;
            }
        }

        /// <summary>
        /// 使用资源管理器打开文件夹路径
        /// </summary>
        /// <param name="dirPath"></param>
        void OpenFolderByDirectoryPath(DirectoryInfo info);

        /// <summary>
        /// 使用资源管理器选中文件路径
        /// </summary>
        /// <param name="filePath"></param>
        void OpenFolderSelectFilePath(FileInfo info);

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
        string? GetRegistryVdfPath();

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
        [SupportedOSPlatform("Linux")]
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

        /// <summary>
        /// 获取当前 Windows 系统产品名称，例如 Windows 10 Pro
        /// </summary>
        [SupportedOSPlatform("Windows")]
        string WindowsProductName => "";

        /// <summary>
        /// 获取当前 Windows 系统第四位版本号
        /// </summary>
        [SupportedOSPlatform("Windows")]
        int WindowsVersionRevision => 0;

        /// <summary>
        /// 获取当前 Windows 10/11 系统显示版本，例如 21H1
        /// </summary>
        [SupportedOSPlatform("Windows")]
        string WindowsReleaseIdOrDisplayVersion => "";

        bool IPlatformService.IsMobile => false;
    }
}
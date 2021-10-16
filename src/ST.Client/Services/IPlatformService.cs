using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Application.Models;
using System.Application.Settings;
using Xamarin.Essentials;
#if NETSTANDARD
using JustArchiNET.Madness;
#else
using System.Runtime.Versioning;
#endif

namespace System.Application.Services
{
    /// <summary>
    /// 由平台实现的服务
    /// </summary>
    public partial interface IPlatformService : IService<IPlatformService>
    {
        protected const string TAG = "PlatformS";

        /// <summary>
        /// 运行 Shell 脚本
        /// </summary>
        /// <param name="script">要运行的脚本字符串</param>
        /// <param name="admin">是否以管理员或Root权限运行</param>
        async void RunShell(string script, bool admin = false) => await RunShellAsync(script, admin);

        /// <inheritdoc cref="RunShell(string, bool)"/>
        ValueTask RunShellAsync(string script, bool admin = false)
        {
            return new();
        }

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
        /// 在 Windows 上时使用 .NET Framework 中 <see cref="Encoding.Default"/> 行为。
        /// <para></para>
        /// 非 Windows 上等同于 <see cref="Encoding.Default"/>(UTF8)
        /// </summary>
        Encoding Default => Encoding.Default;

        /// <summary>
        /// 设置启用或关闭系统代理
        /// </summary>
        bool SetAsSystemProxy(bool state, IPAddress? ip = null, int port = -1)
        {
            return false;
        }

        /// <summary>
        /// 获取一个正在运行的进程的命令行参数
        /// 与 <see cref="Environment.GetCommandLineArgs"/> 一样，使用此方法获取的参数是包含应用程序路径的
        /// 关于 <see cref="Environment.GetCommandLineArgs"/> 可参见：
        /// .NET 命令行参数包含应用程序路径吗？https://blog.walterlv.com/post/when-will-the-command-line-args-contain-the-executable-path.html
        /// </summary>
        /// <param name="process">一个正在运行的进程</param>
        /// <returns>表示应用程序运行命令行参数的字符串</returns>
        string GetCommandLineArgs(Process process) => string.Empty;

        void SetResizeMode(IntPtr hWnd, ResizeMode value) { }

        /// <summary>
        /// hosts 文件所在目录
        /// </summary>
        string HostsFilePath => string.Format("{0}etc{0}hosts", Path.DirectorySeparatorChar);

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
        void OpenFolderByDirectoryPath(DirectoryInfo info)
        {

        }

        /// <summary>
        /// 使用资源管理器选中文件路径
        /// </summary>
        /// <param name="filePath"></param>
        void OpenFolderSelectFilePath(FileInfo info)
        {

        }

        /// <summary>
        /// 设置系统关闭时任务
        /// </summary>
        void SetSystemSessionEnding(Action action)
        {

        }

        /// <summary>
        /// 获取文本阅读器提供商程序文件路径或文件名(如果提供程序已注册环境变量)
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        string? GetFileName(TextReaderProvider provider)
        {
            return null;
        }

        /// <summary>
        /// 设置开机自启动
        /// </summary>
        /// <param name="isAutoStart">开启<see langword="true"/>、关闭<see langword="false"/></param>
        /// <param name="name"></param>
        void SetBootAutoStart(bool isAutoStart, string name)
        {

        }

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

        protected static (byte[] key, byte[] iv) GetMachineSecretKeyBySecureStorage()
        {
            if (!Essentials.IsSupported) throw new PlatformNotSupportedException();
            const string KEY_MACHINE_SECRET = "KEY_MACHINE_SECRET_2105";
            var guid = GetMachineSecretKeyGuid();
            static Guid GetMachineSecretKeyGuid()
            {
                Func<Task<string>> getAsync = () => SecureStorage.GetAsync(KEY_MACHINE_SECRET);
                var guidStr = getAsync.RunSync();
                if (Guid.TryParse(guidStr, out var guid)) return guid;
                guid = Guid.NewGuid();
                guidStr = guid.ToString();
                Func<Task> setAsync = () => SecureStorage.SetAsync(KEY_MACHINE_SECRET, guidStr);
                setAsync.RunSync();
                return guid;
            }
            var r = AESUtils.GetParameters(guid.ToByteArray());
            return r;
        }

        protected static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKeyBySecureStorage = new(GetMachineSecretKeyBySecureStorage);
        (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKeyBySecureStorage.Value;

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
        /// 当前程序是否以 Administrator 或 Root 权限运行
        /// </summary>
        bool IsAdministrator => false;
    }
}
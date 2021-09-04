using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
#if !NET35 && !NOT_XE
using System.Threading.Tasks;
using Xamarin.Essentials;
#endif

namespace System
{
    /// <summary>
    /// [文件夹(Dir)]、[文件(File)]的[路径/目录(Path)]相关的工具类
    /// </summary>
    public static class IOPath
    {
        #region 文件夹(Dir)

        /// <summary>
        /// 如果指定的[文件夹(Dir)]目录不存在，则创建[文件夹(Dir)]目录
        /// </summary>
        /// <param name="dirPath"></param>
        public static void DirCreateByNotExists(string dirPath)
        {
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        }

        /// <inheritdoc cref="DirCreateByNotExists(string)"/>
        public static void CreateByNotExists(this DirectoryInfo dirInfo)
        {
            if (!dirInfo.Exists) dirInfo.Create();
        }

        /// <summary>
        /// 尝试删除指定的[文件夹(Dir)]路径，默认将删除文件夹下的所有文件、子目录
        /// <para>通常用于删除缓存</para>
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="noRecursive"></param>
        public static void DirTryDelete(string dirPath, bool noRecursive = false)
        {
            try
            {
                Directory.Delete(dirPath, !noRecursive);
            }
            catch
            {
            }
        }

        #endregion

        #region 文件(File)

        /// <summary>
        /// 如果指定的[文件(File)]路径存在，则删除
        /// <para>可选择是否检查所在[文件夹(Dir)]路径是否存在，不存在则创建[文件夹(Dir)]</para>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="notCreateDir"></param>
        public static void FileIfExistsItDelete(string filePath, bool notCreateDir = false)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else if (!notCreateDir)
            {
                DirCreateByNotExists(Path.GetDirectoryName(filePath));
            }
        }

        /// <inheritdoc cref="FileIfExistsItDelete(string, bool)"/>
        public static void IfExistsItDelete(this FileInfo fileInfo, bool notCreateDir = false)
        {
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            else if (!notCreateDir)
            {
                DirCreateByNotExists(Path.GetDirectoryName(fileInfo.FullName));
            }
        }

        /// <summary>
        /// 尝试删除指定的[文件(File)]路径
        /// <para>通常用于删除缓存</para>
        /// </summary>
        /// <param name="filePath"></param>
        public static void FileTryDelete(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
            }
        }

        #endregion

        #region FileSystem

        public const string DirName_AppData = "AppData";
        public const string DirName_Cache = "Cache";

        static Func<string>? getAppDataDirectory;
        static Func<string>? getCacheDirectory;

        /// <inheritdoc cref="FileSystem.AppDataDirectory"/>
        public static string AppDataDirectory
        {
            get
            {
                if (getAppDataDirectory != null)
                    return getAppDataDirectory();
#if NET35 || NOT_XE
                throw new PlatformNotSupportedException();
#else
                return FileSystem.AppDataDirectory;
#endif
            }
        }

        /// <inheritdoc cref="FileSystem.CacheDirectory"/>
        public static string CacheDirectory
        {
            get
            {
                if (getCacheDirectory != null)
                    return getCacheDirectory();
#if NET35 || NOT_XE
                throw new PlatformNotSupportedException();
#else
                return FileSystem.CacheDirectory;
#endif
            }
        }

        /// <summary>
        /// (可选)初始化文件系统
        /// <para>通常在 Xamarin.Essentials 不支持的平台上，为必选项</para>
        /// </summary>
        /// <param name="getAppDataDirectory">获取应用目录文件夹</param>
        /// <param name="getCacheDirectory">获取缓存目录文件夹</param>
        public static void InitFileSystem(Func<string> getAppDataDirectory, Func<string> getCacheDirectory)
        {
            IOPath.getAppDataDirectory = getAppDataDirectory;
            IOPath.getCacheDirectory = getCacheDirectory;
        }

        #endregion

#if !NET35
        #region BaseDirectory

        public const string DirName_Bin = "Bin";

        static readonly Lazy<string> _BaseDirectory = new(() =>
        {
            var value = AppContext.BaseDirectory;
            if (OperatingSystem2.IsWindows && !DesktopBridge.IsRunningAsUwp) // 启用将发布 Host 入口点重定向到 Bin 目录中时重定向基目录
            {
                var value2 = new DirectoryInfo(value);
                if (value2.Parent != null && string.Equals(value2.Name, DirName_Bin, StringComparison.OrdinalIgnoreCase))
                {
                    value = value2.Parent.FullName;
                }
            }
            return value;
        });

        public static string BaseDirectory =>
        //AppContext.BaseDirectory;
        _BaseDirectory.Value;

        #endregion
#endif

        /// <summary>
        /// 获取[文件(File)]资源路径
        /// </summary>
        /// <param name="resData">资源数据</param>
        /// <param name="resName">资源名称</param>
        /// <param name="resVer">资源文件版本</param>
        /// <param name="fileEx">资源文件扩展名</param>
        /// <returns></returns>
        public static string GetFileResourcePath(byte[] resData, string resName, int resVer, string fileEx)
        {
            var dirPath = Path.Combine(AppDataDirectory, resName);
            var filePath = Path.Combine(dirPath, $"{resName}@{resVer}{fileEx}");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                WriteFile();
            }
            else
            {
                if (!File.Exists(filePath))
                {
                    var oldFiles = Directory.GetFiles(dirPath);
                    if (oldFiles != null)
                    {
                        foreach (var oldFile in oldFiles)
                        {
                            FileTryDelete(oldFile);
                        }
                    }
                    WriteFile();
                }
            }
            void WriteFile() => File.WriteAllBytes(filePath, resData);
            return filePath;
        }

        /// <summary>
        /// 判断路径是否为[文件夹(Dir)]，返回 <see cref="FileInfo"/> 或 <see cref="DirectoryInfo"/>，<see langword="true"/> 为文件夹，<see langword="false"/> 为文件，路径不存在则为 <see langword="null"/>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileInfo"></param>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        public static bool? IsDirectory(string path, [NotNullWhen(false)] out FileInfo? fileInfo, [NotNullWhen(true)] out DirectoryInfo? directoryInfo)
        {
            fileInfo = new(path);
            if (fileInfo.Exists)
            {
                directoryInfo = null;
                return false;
            }
            fileInfo = null;
            directoryInfo = new(path);
            if (directoryInfo.Exists)
            {
                return true;
            }
            directoryInfo = null;
            return false;
        }

        static FileStream OpenReadCore(string filePath) => new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

        public static FileStream? OpenRead(string? filePath)
        {
            if (filePath == null) return null;
            //var fileStream = OpenReadCore(filePath);
            TryOpenRead(filePath, out var stream, out var ex);
            if (ex != null)
                Log.Error(nameof(OpenRead), ex, "OpenRead Error");
            return stream;
        }

#if !NET35
        static bool TryCall<T>(string? filePath, [NotNullWhen(true)] out T? t, out Exception? ex, Func<string, T> func) where T : class
        {
            ex = null;
            t = null;
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    t = func(filePath);
                    return true;
                }
                catch (Exception e)
                {
                    ex = e;
                }
            }
            return false;
        }

        public static bool TryOpenRead(string? filePath, [NotNullWhen(true)] out FileStream? fileStream, out Exception? ex) => TryCall(filePath, out fileStream, out ex, OpenReadCore);

        public static bool TryOpen(string? filePath, FileMode mode, FileAccess access, FileShare share, [NotNullWhen(true)] out FileStream? fileStream, out Exception? ex) => TryCall(filePath, out fileStream, out ex, p => new(p, mode, access, share));

        public static bool TryReadAllBytes(string? filePath, [NotNullWhen(true)] out byte[]? byteArray, out Exception? ex) => TryCall(filePath, out byteArray, out ex, File.ReadAllBytes);

        public static bool TryReadAllText(string? filePath, [NotNullWhen(true)] out string? content, out Exception? ex) => TryCall(filePath, out content, out ex, File.ReadAllText);

        public static bool TryReadAllText(string? filePath, Encoding encoding, [NotNullWhen(true)] out string? content, out Exception? ex) => TryCall(filePath, out content, out ex, p => File.ReadAllText(p, encoding));

        public static bool TryReadAllLines(string? filePath, [NotNullWhen(true)] out string[]? lines, out Exception? ex) => TryCall(filePath, out lines, out ex, File.ReadAllLines);

        public static bool TryReadAllLines(string? filePath, Encoding encoding, [NotNullWhen(true)] out string[]? lines, out Exception? ex) => TryCall(filePath, out lines, out ex, p => File.ReadAllLines(p, encoding));

        public static bool TryReadAllLines(string? filePath, [NotNullWhen(true)] out IEnumerable<string>? lines, out Exception? ex) => TryCall(filePath, out lines, out ex, File.ReadLines);

        public static bool TryReadAllLines(string? filePath, Encoding encoding, [NotNullWhen(true)] out IEnumerable<string>? lines, out Exception? ex) => TryCall(filePath, out lines, out ex, p => File.ReadLines(p, encoding));

        static async Task<(bool success, T? byteArray, Exception? ex)> TryCallAsync<T>(string? filePath, CancellationToken cancellationToken, Func<string, CancellationToken, Task<T>> func) where T : class
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    var t = await func(filePath, cancellationToken);
                    return (true, t, null);
                }
                catch (Exception e)
                {
                    return (false, null, e);
                }
            }
            return (false, null, null);
        }

        public static Task<(bool success, byte[]? byteArray, Exception? ex)> TryReadAllBytesAsync(string? filePath, CancellationToken cancellationToken = default) => TryCallAsync(filePath, cancellationToken, File.ReadAllBytesAsync);

        public static Task<(bool success, string[]? lines, Exception? ex)> TryReadAllLinesAsync(string? filePath, CancellationToken cancellationToken = default) => TryCallAsync(filePath, cancellationToken, File.ReadAllLinesAsync);

        public static Task<(bool success, string[]? lines, Exception? ex)> TryReadAllLinesAsync(string? filePath, Encoding encoding, CancellationToken cancellationToken = default) => TryCallAsync(filePath, cancellationToken, (p, tk) => File.ReadAllLinesAsync(p, encoding, tk));

        public static Task<(bool success, string? content, Exception? ex)> TryReadAllTextAsync(string? filePath, CancellationToken cancellationToken = default) => TryCallAsync(filePath, cancellationToken, File.ReadAllTextAsync);

        public static Task<(bool success, string? content, Exception? ex)> TryReadAllTextAsync(string? filePath, Encoding encoding, CancellationToken cancellationToken = default) => TryCallAsync(filePath, cancellationToken, (p, tk) => File.ReadAllTextAsync(p, encoding, tk));
#endif

        /// <summary>
        /// 获取文件的大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>单位 字节</returns>
        public static double GetFileSize(FileInfo fileInfo) => fileInfo.Exists ? fileInfo.Length : 0D;

        /// <summary>
        /// 获取指定路径的大小
        /// </summary>
        /// <param name="dirPath">路径</param>
        /// <returns>单位 字节</returns>
        public static double GetDirectorySize(string dirPath)
        {
            double len = 0D;
            // 判断该路径是否存在（是否为文件夹）
            var isDirectory = IsDirectory(dirPath, out var fileInfo, out var directoryInfo);
            if (isDirectory.HasValue)
            {
                if (!isDirectory.Value)
                {
                    //查询文件的大小
                    len = GetFileSize(fileInfo!);
                }
                else
                {
                    // 通过GetFiles方法，获取di目录中的所有文件的大小
                    len += directoryInfo!.GetFiles().Sum(x => x.Length);
                    // 获取di中所有的文件夹，并存到一个新的对象数组中，以进行递归
                    len += directoryInfo.GetDirectories().Sum(x => GetDirectorySize(x.FullName));
                }
            }
            return len;
        }

        const double unit = 1024d;
        static readonly string[] units = new[] { "B", "KB", "MB", "GB", "TB" };

        public static (double length, string unit) GetSize(double length)
        {
            if (length > 0d)
            {
                for (int i = 0; i < units.Length; i++)
                {
                    if (i > 0) length /= unit;
                    if (length < unit) return (length, units[i]);
                }
                return (length, units.Last());
            }
            else
            {
                return (0, units.First());
            }
        }

        public static string GetSizeString(double length)
        {
            (length, string unit) = GetSize(length);
            return $"{length:0.00} {unit}";
        }
    }
}
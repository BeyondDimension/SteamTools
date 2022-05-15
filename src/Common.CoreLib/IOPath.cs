using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.Versioning;
#if !NET35 && !NOT_XE
using System.Threading.Tasks;
#endif
using CC = System.Common.Constants;

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
        public static string DirCreateByNotExists(string dirPath)
        {
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            return dirPath;
        }

        /// <inheritdoc cref="DirCreateByNotExists(string)"/>
        public static DirectoryInfo CreateByNotExists(this DirectoryInfo dirInfo)
        {
            if (!dirInfo.Exists) dirInfo.Create();
            return dirInfo;
        }

        /// <summary>
        /// 尝试删除指定的[文件夹(Dir)]路径，默认将删除文件夹下的所有文件、子目录
        /// <para>通常用于删除缓存</para>
        /// </summary>
        /// <param name="dirPath"></param>
        /// <param name="noRecursive"></param>
        public static bool DirTryDelete(string dirPath, bool noRecursive = false)
        {
            try
            {
                Directory.Delete(dirPath, !noRecursive);
                return true;
            }
            catch
            {
                return false;
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
                var dirName = Path.GetDirectoryName(filePath);
                if (dirName != null)
                {
                    DirCreateByNotExists(dirName);
                }
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
                var dirName = Path.GetDirectoryName(fileInfo.FullName);
                if (dirName != null)
                {
                    DirCreateByNotExists(dirName);
                }
            }
        }

        /// <summary>
        /// 尝试删除指定的[文件(File)]路径
        /// <para>通常用于删除缓存</para>
        /// </summary>
        /// <param name="filePath"></param>
        public static bool FileTryDelete(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region FileSystem

        public const string DirName_AppData = "AppData";
        public const string DirName_Cache = "Cache";

        static Func<string>? getAppDataDirectory;
        static Func<string>? getCacheDirectory;

        /// <summary>
        /// 必须在 main 函数中初始化文件夹目录，否则将在使用时抛出此异常
        /// </summary>
        static Exception MustCallFileSystemInitException => new NullReferenceException("msut call FileSystemXXX.InitFileSystem(..");

        /// <summary>
        /// 获取可存储应用程序数据的位置
        /// </summary>
        public static string AppDataDirectory
        {
            get
            {
                if (getAppDataDirectory != null)
                    return getAppDataDirectory();
                throw MustCallFileSystemInitException;
            }
        }

        /// <summary>
        /// 获取可以存储临时数据的位置
        /// </summary>
        public static string CacheDirectory
        {
            get
            {
                if (getCacheDirectory != null)
                    return getCacheDirectory();
                throw MustCallFileSystemInitException;
            }
        }

        public abstract class FileSystemBase
        {
            protected FileSystemBase()
            {
            }

            /// <summary>
            /// 初始化文件系统
            /// </summary>
            /// <param name="getAppDataDirectory">获取应用目录文件夹</param>
            /// <param name="getCacheDirectory">获取缓存目录文件夹</param>
            protected static void InitFileSystem(Func<string> getAppDataDirectory, Func<string> getCacheDirectory)
            {
                IOPath.getAppDataDirectory = getAppDataDirectory;
                IOPath.getCacheDirectory = getCacheDirectory;
            }

            /// <summary>
            /// 带迁移的初始化文件系统，使用 <see cref="Directory.Move(string, string)"/> 或 xcopy 进行移动，如果迁移失败则回退源目录
            /// </summary>
            /// <param name="destAppDataPath">新的 AppData 文件夹路径</param>
            /// <param name="destCachePath">新的 Cache 文件夹路径</param>
            /// <param name="sourceAppDataPath">旧的 AppData 文件夹路径</param>
            /// <param name="sourceCachePath">旧的 Cache 文件夹路径</param>
            protected static void InitFileSystemWithMigrations(
                string destAppDataPath, string destCachePath,
                string sourceAppDataPath, string sourceCachePath)
            {
                bool ExistsNotEmptyDir(string path)
                {
                    var exists = Directory.Exists(path);
                    if (DesktopBridge.IsRunningAsUwp)
                    {
                        if (path == destCachePath || path == sourceCachePath)
                        {
                            return false;
                        }
                    }
                    return exists && Directory.EnumerateFileSystemEntries(path).Any(); // 文件夹存在且不为空文件夹
                }

                var paths = new[] { destAppDataPath, destCachePath, };
                var dict_paths = paths.ToDictionary(x => x, x => ExistsNotEmptyDir(x));

                if (dict_paths.Values.All(x => !x))
                {
                    var old_paths = new[] { sourceAppDataPath, sourceCachePath, };
                    if (old_paths.All(x => Directory.Exists(x) && Directory.EnumerateFileSystemEntries(x).Any())) // 迁移之前根目录上的文件夹
                    {
                        var isNotFirst = false;
                        for (int i = 0; i < old_paths.Length; i++)
                        {
                            var path = paths[i];
                            var old_path = old_paths[i];
                            try
                            {
                                if (!isNotFirst)
                                {
                                    try
                                    {
                                        // 尝试搜索之前版本的进程将其结束
                                        var currentProcess = Process.GetCurrentProcess();
                                        var query = from x in Process.GetProcessesByName(currentProcess.ProcessName)
                                                    where x != currentProcess
                                                    let m = x.TryGetMainModule()
                                                    where m != null && m.FileName != currentProcess.TryGetMainModule()?.FileName
                                                    select x;
                                        var process = query.ToArray();
                                        foreach (var proces in process)
                                        {
                                            try
                                            {
#if NETCOREAPP3_0_OR_GREATER
                                                proces.Kill(true);
#else
                                                proces.Kill();
#endif
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                    isNotFirst = true;
                                }
                                MoveDirectory(old_path, path);
                                dict_paths[path] = true;
                            }
                            catch
                            {
                                if (!DesktopBridge.IsRunningAsUwp)
                                {
                                    // 跨卷移动失败或其他原因失败，使用旧的目录，并尝试删除创建的空文件夹
                                    DirTryDelete(path);
                                }
                                paths[i] = old_path;
                            }
                        }
                    }
                }

                foreach (var item in dict_paths)
                {
                    if (!item.Value)
                    {
                        Directory.CreateDirectory(item.Key);
                    }
                }

                InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
                string GetAppDataDirectory() => paths[0];
                string GetCacheDirectory() => paths[1];
            }

            /// <summary>
            /// 初始化文件系统，但优先使用旧目录上的文件夹，如果存在的话(允许空文件夹)，不会进行文件迁移
            /// </summary>
            /// <param name="destAppDataPath">新的 AppData 文件夹路径</param>
            /// <param name="destCachePath">新的 Cache 文件夹路径</param>
            /// <param name="sourceAppDataPath">旧的 AppData 文件夹路径</param>
            /// <param name="sourceCachePath">旧的 Cache 文件夹路径</param>
            protected static void InitFileSystemUseDestFirst(
                string destAppDataPath, string destCachePath,
                string sourceAppDataPath, string sourceCachePath)
            {
                var paths = new[] { destAppDataPath, destCachePath, };
                var old_paths = new[] { sourceAppDataPath, sourceCachePath, };

                for (int i = 0; i < old_paths.Length; i++)
                {
                    var item = old_paths[i];
                    if (Directory.Exists(item))
                    {
                        paths[i] = item;
                    }
                }

                InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
                string GetAppDataDirectory() => paths[0];
                string GetCacheDirectory() => paths[1];
            }
        }

        /// <summary>
        /// (可选)初始化文件系统
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

        //static readonly Lazy<string> _BaseDirectory = new(() =>
        //{
        //    var value = AppContext.BaseDirectory;
        //    if (OperatingSystem2.IsWindows && !DesktopBridge.IsRunningAsUwp) // 启用将发布 Host 入口点重定向到 Bin 目录中时重定向基目录
        //    {
        //        var value2 = new DirectoryInfo(value);
        //        if (value2.Parent != null && string.Equals(value2.Name, "Bin", StringComparison.OrdinalIgnoreCase))
        //        {
        //            value = value2.Parent.FullName;
        //        }
        //    }
        //    return value;
        //});

        public static string BaseDirectory =>
        AppContext.BaseDirectory;
        //_BaseDirectory.Value;

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
            if (fileInfo.Exists) // 路径为文件
            {
                directoryInfo = null;
                return false;
            }
            fileInfo = null;
            directoryInfo = new(path);
            if (directoryInfo.Exists) // 路径为文件夹
            {
                return true;
            }
            directoryInfo = null;
            return null;
        }

        /// <summary>
        /// 允许文件共享 <see cref="FileShare.Read"/> OR <see cref="FileShare.Write"/> OR <see cref="FileShare.Delete"/>
        /// </summary>
        public const FileShare FileShareReadWriteDelete = FileShare.ReadWrite | FileShare.Delete;

        static FileStream OpenReadCore(string filePath) => new(filePath, FileMode.Open, FileAccess.Read, FileShareReadWriteDelete);

        /// <summary>
        /// 尝试打开文件流，使用 <see cref="FileShareReadWriteDelete"/>，打开失败时将返回 <see langword="null"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="writeLog">是否在失败时纪录日志</param>
        /// <returns></returns>
        public static FileStream? OpenRead(string? filePath, bool writeLog = true)
        {
            if (filePath == null) return null;
            TryOpenRead(filePath, out var stream, out var ex);
            if (writeLog && ex != null)
                Log.Error(nameof(OpenRead), ex, $"OpenRead Error, filePath: {filePath}");
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

        /// <summary>
        /// 尝试打开文件流，使用 <see cref="FileShareReadWriteDelete"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileStream"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool TryOpenRead(string? filePath, [NotNullWhen(true)] out FileStream? fileStream, out Exception? ex) => TryCall(filePath, out fileStream, out ex, OpenReadCore);

        /// <summary>
        /// 尝试打开文件流
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="mode"></param>
        /// <param name="access"></param>
        /// <param name="share"></param>
        /// <param name="fileStream"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool TryOpen(string? filePath, FileMode mode, FileAccess access, FileShare share, [NotNullWhen(true)] out FileStream? fileStream, out Exception? ex) => TryCall(filePath, out fileStream, out ex, p => new(p, mode, access, share));

        public static bool TryReadAllBytes(string? filePath, [NotNullWhen(true)] out byte[]? byteArray, out Exception? ex) => TryCall(filePath, out byteArray, out ex, File.ReadAllBytes);

        public static bool TryReadAllText(string? filePath, [NotNullWhen(true)] out string? content, out Exception? ex) => TryCall(filePath, out content, out ex, File.ReadAllText);

        public static bool TryReadAllText(string? filePath, Encoding encoding, [NotNullWhen(true)] out string? content, out Exception? ex) => TryCall(filePath, out content, out ex, p => File.ReadAllText(p, encoding));

        public static bool TryReadAllLines(string? filePath, [NotNullWhen(true)] out string[]? lines, out Exception? ex) => TryCall(filePath, out lines, out ex, File.ReadAllLines);

        public static bool TryReadAllLines(string? filePath, Encoding encoding, [NotNullWhen(true)] out string[]? lines, out Exception? ex) => TryCall(filePath, out lines, out ex, p => File.ReadAllLines(p, encoding));

        public static bool TryReadAllLines(string? filePath, [NotNullWhen(true)] out IEnumerable<string>? lines, out Exception? ex) => TryCall(filePath, out lines, out ex, File.ReadLines);

        public static bool TryReadAllLines(string? filePath, Encoding encoding, [NotNullWhen(true)] out IEnumerable<string>? lines, out Exception? ex) => TryCall(filePath, out lines, out ex, p => File.ReadLines(p, encoding));

        static async Task<(bool success, T? data, Exception? ex)> TryCallAsync<T>(string? filePath, Func<string, CancellationToken, Task<T>> func, CancellationToken cancellationToken) where T : class
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                try
                {
                    T data;
#pragma warning disable CA2208 // 正确实例化参数异常
                    data = await func(filePath, cancellationToken) ?? throw new ArgumentNullException(nameof(data));
#pragma warning restore CA2208 // 正确实例化参数异常
                    return (true, data, null);
                }
                catch (Exception e)
                {
                    return (false, null, e);
                }
            }
            return (false, null, null);
        }

        public static Task<(bool success, byte[]? byteArray, Exception? ex)> TryReadAllBytesAsync(string? filePath, CancellationToken cancellationToken = default) => TryCallAsync(filePath, File.ReadAllBytesAsync, cancellationToken);

        public static Task<(bool success, string[]? lines, Exception? ex)> TryReadAllLinesAsync(string? filePath, CancellationToken cancellationToken = default) => TryCallAsync(filePath, File.ReadAllLinesAsync, cancellationToken);

        public static Task<(bool success, string[]? lines, Exception? ex)> TryReadAllLinesAsync(string? filePath, Encoding encoding, CancellationToken cancellationToken = default) => TryCallAsync(filePath, (p, tk) => File.ReadAllLinesAsync(p, encoding, tk), cancellationToken);

        public static Task<(bool success, string? content, Exception? ex)> TryReadAllTextAsync(string? filePath, CancellationToken cancellationToken = default) => TryCallAsync(filePath, File.ReadAllTextAsync, cancellationToken);

        public static Task<(bool success, string? content, Exception? ex)> TryReadAllTextAsync(string? filePath, Encoding encoding, CancellationToken cancellationToken = default) => TryCallAsync(filePath, (p, tk) => File.ReadAllTextAsync(p, encoding, tk), cancellationToken);
#endif

        /// <summary>
        /// 获取文件的大小
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns>单位 字节</returns>
        public static decimal GetFileSize(FileInfo fileInfo) => fileInfo.Exists ? fileInfo.Length : 0M;

        /// <summary>
        /// 获取指定路径的大小
        /// </summary>
        /// <param name="dirPath">路径</param>
        /// <returns>单位 字节</returns>
        public static decimal GetDirectorySize(string dirPath)
        {
            var len = 0M;
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

        const decimal unit = 1024M;
        static readonly string[] units = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB", "BB" };

        public static (decimal length, string unit) GetSize(decimal length)
        {
            if (length > 0M)
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
                return (0M, units.First());
            }
        }

        public static string GetSizeString(decimal length)
        {
            (length, string unit) = GetSize(length);
            return $"{length:0.00} {unit}";
        }

        public static int GetProgressPercentage(decimal current, decimal total)
        {
            decimal t = decimal.Parse((current / total).ToString("0.000"));

            var t1 = Math.Round(t, 2);
            return Convert.ToInt32(t1 * 100);
        }

        public const char UnixDirectorySeparatorChar = '/';
        public const char WinDirectorySeparatorChar = '\\';

        /// <summary>
        /// 将现有文件夹复制到新文件夹
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        /// <param name="recursive"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        /// <summary>
        /// 将现有文件夹复制到新文件夹(xcopy)
        /// <para>https://docs.microsoft.com/zh-cn/windows-server/administration/windows-commands/xcopy</para>
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="timeoutMilliseconds"></param>
        [SupportedOSPlatform("Windows")]
        public static void XCopyDirectory(string sourceDirName, string destDirName, int timeoutMilliseconds = 60000)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
            };
            using var p = Process.Start(psi);
            p.Start();
            p.StandardInput.WriteLine($"xcopy \"{sourceDirName}\" \"{destDirName}\" /y &exit");
            p.WaitForExit(timeoutMilliseconds);
            p.Kill();
        }

        /// <summary>
        /// 将文件或目录及其内容移动或复制到新位置
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        public static void MoveDirectory(string sourceDirName, string destDirName)
        {
            try
            {
                Directory.Move(sourceDirName, destDirName);
            }
            catch
            {
                try
                {
                    CopyDirectory(sourceDirName, destDirName);
                }
                catch
                {
                    if (OperatingSystem2.IsWindows)
                    {
                        XCopyDirectory(sourceDirName, destDirName);
                    }
                    else
                    {
                        throw;
                    }
                }
                try
                {
                    Directory.Delete(sourceDirName, true);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// 将文件或目录及其内容移动或复制到新位置(异步与带进度)
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static async Task MoveDirectoryAsync(string sourceDirName, string destDirName, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
        {
            const float maxProgress = CC.MaxProgress;
            try
            {
                Directory.Move(sourceDirName, destDirName);
            }
            catch
            {
                float totalSize = 0f;
                float currentSize = 0f;
                var streams = new List<FileStream>();
                try
                {
                    var tasks = new List<Task>();
                    CopyDirectoryTaskList(sourceDirName, destDirName);
                    if (progress != null) StartObserver();
                    await Task.WhenAll(tasks);
                    currentSize = totalSize;

                    async void StartObserver(int millisecondsDelay = 1500)
                    {
                        do
                        {
                            try
                            {
                                await Task.Delay(millisecondsDelay, cancellationToken);
                                progress.Report(currentSize / totalSize);
                            }
                            catch (OperationCanceledException)
                            {
                                return;
                            }
                        } while (currentSize < totalSize);
                    }

                    void CopyDirectoryTaskList(string sourceDir, string destinationDir)
                    {
                        // Get information about the source directory
                        var dir = new DirectoryInfo(sourceDir);

                        // Check if the source directory exists
                        if (!dir.Exists)
                            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

                        // Cache directories before we start copying
                        DirectoryInfo[] dirs = dir.GetDirectories();

                        // Create the destination directory
                        if (!Directory.Exists(destinationDir))
                            Directory.CreateDirectory(destinationDir);

                        async Task CopyFileAsync(FileInfo file)
                        {
                            float fileSize = 0;
                            FileStream? sourceStream = null, destStream = null;
                            try
                            {
                                sourceStream = file.Open(FileMode.Open, FileAccess.Read, FileShareReadWriteDelete);
                                fileSize = sourceStream.Length;
                                streams.Add(sourceStream);

                                string targetFilePath = Path.Combine(destinationDir, file.Name);
                                destStream = new FileStream(targetFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShareReadWriteDelete);

                                await sourceStream.CopyToAsync(destStream, cancellationToken);
                                await destStream.FlushAsync(cancellationToken);
                                destStream.SetLength(destStream.Position);
                            }
                            finally
                            {
                                if (sourceStream != null)
                                {
                                    sourceStream.Dispose();
                                    streams.Remove(sourceStream);
                                }
                                if (destStream != null)
                                {
                                    destStream.Dispose();
                                    streams.Remove(destStream);
                                }
                                currentSize += fileSize;
                            }
                        }

                        // Get the files in the source directory and copy to the destination directory
                        foreach (FileInfo file in dir.GetFiles())
                        {
                            totalSize += file.Length;
                            var task = CopyFileAsync(file);
                            tasks.Add(task);
                        }

                        // If recursive and copying subdirectories, recursively call this method
                        foreach (DirectoryInfo subDir in dirs)
                        {
                            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                            CopyDirectoryTaskList(subDir.FullName, newDestinationDir);
                        }
                    }
                }
                finally
                {
                    foreach (var stream in streams)
                    {
                        stream.Dispose();
                    }
                }
            }
            progress?.Report(maxProgress);
        }
    }
}
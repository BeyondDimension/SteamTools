#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// <list type="bullet">
/// <item>AppData: \AppData or %LocalAppData%\Steam++</item>
/// <item>Cache: \Cache or %Tmp%\Steam++</item>
/// <item>Logs: \Logs or %Tmp%\Steam++\Logs</item>
/// </list>
/// </summary>
sealed class WindowsFileSystem : IOPath.FileSystemBase
{
    private WindowsFileSystem() => throw new NotSupportedException();

    /// <inheritdoc cref="FileSystem2.InitFileSystem"/>
    public static void InitFileSystem()
    {
        var isPrivilegedProcess = WindowsPlatformServiceImpl.IsPrivilegedProcess;
        if (isPrivilegedProcess)
        {
            // 删除早期版本在根目录下的本机库，避免因旧版本文件存在，可能从根目录上加载了旧的本机库
            var rootDeleteFiles = new[]
            {
                "7z.dll",
                "aspnetcorev2_inprocess.dll",
                "av_libglesv2.dll",
                "e_sqlite3.dll",
                "libHarfBuzzSharp.dll",
                "libSkiaSharp.dll",
                "WebView2Loader.dll",
                "WinDivert.dll",
                "WinDivert64.sys",
            };
            foreach (var item in rootDeleteFiles)
            {
                var path = Path.Combine(IOPath.BaseDirectory, item);
                IOPath.FileTryDelete(path);
            }
        }

        if (WindowsPlatformServiceImpl.CurrentAppIsInstallVersion)
        {
            /* 安装版将使用以下路径，但如果根目录上有文件夹则会优先使用根目录上的文件夹(从 2.7.0+ 开始)
             * Environment.SpecialFolder.LocalApplicationData
             * Path.GetTempPath()
             */
            InitFileSystemByInstallVersion();
            return;
        }
        FileSystem2.InitFileSystem();
    }

    /// <summary>
    /// 初始化文件系统，将旧目录上的文件夹复制到新的上，并使用新的
    /// </summary>
    /// <param name="destAppDataPath">新的 AppData 文件夹路径</param>
    /// <param name="destCachePath">新的 Cache 文件夹路径</param>
    /// <param name="sourceAppDataPath">旧的 AppData 文件夹路径</param>
    /// <param name="sourceCachePath">旧的 Cache 文件夹路径</param>
    static void InitFileSystemByInstallVersion()
    {
        var appDataDirectory = AppDataDirectory;
        var cacheDirectory = CacheDirectory;

        DirectoryInfo oldAppDataDirectoryInfo = new(FileSystem2.BaseDirectory.AppDataDirectory);
        if (oldAppDataDirectoryInfo.Exists)
        {
            static void EnumerateDbFiles(
                string oldAppData,
                string newAppData,
                string[] dbFiles,
                bool isCopyOrDelete)
            {
                var item = dbFiles[0];
                var dbFilePath = Path.Combine(oldAppData, item);
                if (File.Exists(dbFilePath))
                {
                    var destFilePath = Path.Combine(newAppData, item);
                    var hashFilePath = destFilePath + "_hash.tmp";
                    if (isCopyOrDelete)
                    {
                        if (File.Exists(destFilePath))
                        {
                            return; // 数据库文件存在时，忽略
                        }
                        else
                        {
                            // 复制文件并记录 Hash
                            if (File.Exists(hashFilePath))
                            {
                                // Hash 文件存在，也忽略
                                return;
                            }
                            var hash = CalcHashData(dbFilePath);
                            File.WriteAllBytes(hashFilePath, hash);
                            File.Copy(dbFilePath, destFilePath, true);
                            foreach (var item2 in dbFiles.Skip(1))
                            {
                                dbFilePath = Path.Combine(oldAppData, item2);
                                destFilePath = Path.Combine(newAppData, item2);
                                File.Copy(dbFilePath, destFilePath, true);
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(hashFilePath))
                        {
                            var hash = CalcHashData(dbFilePath);
                            var hash2 = File.ReadAllBytes(hashFilePath);
                            if (hash.SequenceEqual(hash2))
                            {
                                IOPath.FileTryDelete(dbFilePath);
                                foreach (var item2 in dbFiles.Skip(1))
                                {
                                    dbFilePath = Path.Combine(oldAppData, item2);
                                    IOPath.FileTryDelete(dbFilePath);
                                }
                                IOPath.FileTryDelete(hashFilePath);
                            }
                        }
                    }

                    static byte[] CalcHashData(string path)
                    {
                        using var fs = IOPath.OpenRead(path);
                        var hashData = fs == null ? Array.Empty<byte>() : Hashs.ByteArray.SHA384(fs);
                        return hashData;
                    }
                }
            }

            bool? isCopyOrDelete = null;
            var isMainProcess = Startup.Instance.IsMainProcess;
            var isPrivilegedProcess = WindowsPlatformServiceImpl.IsPrivilegedProcess;
            if (isMainProcess)
            {
                // 主进程将数据库文件复制到当前存储目录，并标记
                isCopyOrDelete = true;

            }
            else if (isPrivilegedProcess)
            {
                // 有标记的情况则删除
                isCopyOrDelete = false;
            }

            if (isCopyOrDelete.HasValue)
            {
                string[] db1Files = new[]
                {
                    "application2.dbf",
                    "application2.dbf-shm",
                    "application2.dbf-wal",
                };
                EnumerateDbFiles(oldAppDataDirectoryInfo.FullName, appDataDirectory, db1Files, isCopyOrDelete.Value);
                string[] db2Files = new[]
                {
                    "application2.dbf",
                    "application2.dbf-shm",
                    "application2.dbf-wal",
                };
                EnumerateDbFiles(oldAppDataDirectoryInfo.FullName, appDataDirectory, db2Files, isCopyOrDelete.Value);
            }
        }

        InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
        string GetAppDataDirectory() => appDataDirectory;
        string GetCacheDirectory() => cacheDirectory;
    }

    static string AppDataDirectory
    {
        get
        {
            var value = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(value, Constants.HARDCODED_APP_NAME);
        }
    }

    static string CacheDirectory
    {
        get
        {
            var value = Path.GetTempPath();
            return Path.Combine(value, Constants.HARDCODED_APP_NAME);
        }
    }
}
#endif
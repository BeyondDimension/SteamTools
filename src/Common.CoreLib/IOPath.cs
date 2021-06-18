using System.Diagnostics.CodeAnalysis;
using System.IO;
#if !NET35 && !NOT_XE
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
                try
                {
                    return FileSystem.AppDataDirectory;
                }
                catch
                {
                    if (DI.DeviceIdiom == DeviceIdiom.Desktop)
                    {
                        var r = Path.Combine(BaseDirectory, "AppData");
                        DirCreateByNotExists(r);
                        return r;
                    }
                    throw;
                }
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
                try
                {
                    return FileSystem.CacheDirectory;
                }
                catch
                {
                    if (DI.DeviceIdiom == DeviceIdiom.Desktop)
                    {
                        var r = Path.Combine(BaseDirectory, "Cache");
                        DirCreateByNotExists(r);
                        return r;
                    }
                    throw;
                }
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

        static readonly Lazy<string> _BaseDirectory = new(() =>
        {
            var value = AppContext.BaseDirectory;
            var value2 = new DirectoryInfo(value);
            if (value2.Parent != null && string.Equals(value2.Name, "Bin", StringComparison.OrdinalIgnoreCase))
            {
                value = value2.Parent.FullName;
            }
            return value;
        });

        public static string BaseDirectory => _BaseDirectory.Value;

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
        /// 判断路径是否为[文件夹(Dir)]
        /// </summary>
        /// <param name="ioPath"></param>
        /// <returns></returns>
        public static bool IsDirectory(string ioPath)
        {
#if NET35
            throw new PlatformNotSupportedException();
#else
            var attr = File.GetAttributes(ioPath);
            return attr.HasFlag(FileAttributes.Directory);
#endif
        }

        /// <summary>
        /// 获取[文件夹(Dir)]路径
        /// <para>如果传入的参数为[文件(File)]路径，则返回当前所在[文件夹(Dir)]路径</para>
        /// <para>如果传入的参数为[文件夹(Dir)]路径，则直接返回参数</para>
        /// </summary>
        /// <param name="ioPath"></param>
        /// <returns></returns>
        public static string GetDirectoryPath(string ioPath)
        {
            try
            {
                if (!IsDirectory(ioPath))
                {
                    return Path.GetDirectoryName(ioPath) ?? string.Empty;
                }
            }
            catch
            {
            }
            return ioPath;
        }

        [return: NotNullIfNotNull("filePath")]
        public static FileStream? OpenRead(string? filePath)
        {
            if (filePath == null) return null;
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return fileStream;
        }
    }
}
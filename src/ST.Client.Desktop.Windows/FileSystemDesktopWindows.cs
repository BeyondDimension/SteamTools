using System.Application.Services.Implementation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application
{
    public sealed class FileSystemDesktopWindows : IOPath.FileSystemBase
    {
        private FileSystemDesktopWindows() => throw new NotSupportedException();

        /// <inheritdoc cref="FileSystem2.InitFileSystem"/>
        public static void InitFileSystem()
        {
            if (WindowsPlatformServiceImpl.IsInstall)
            {
                /* 安装版将使用以下路径，但如果根目录上有文件夹则会优先使用根目录上的文件夹(从 2.7.0+ 开始)
                 * Environment.SpecialFolder.LocalApplicationData
                 * Path.GetTempPath()
                 */
                InitFileSystemUseDestFirst(
                    AppDataDirectory,
                    CacheDirectory,
                    FileSystem2.BaseDirectory.AppDataDirectory,
                    FileSystem2.BaseDirectory.CacheDirectory);
                return;
            }
            FileSystem2.InitFileSystem();
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
}

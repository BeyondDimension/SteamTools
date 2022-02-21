using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application
{
    /// <summary>
    /// 适用于 Linux 的文件系统初始化
    /// <para>XDG Base Directory Specification</para>
    /// <para>https://specifications.freedesktop.org/basedir-spec/basedir-spec-latest.html</para>
    /// <para>https://wiki.archlinux.org/title/XDG_Base_Directory</para>
    /// </summary>
    public sealed class FileSystemDesktopXDG : IOPath.FileSystemBase
    {
        private FileSystemDesktopXDG() => throw new NotSupportedException();

        /// <inheritdoc cref="FileSystem2.InitFileSystem"/>
        public static void InitFileSystem()
        {
            InitFileSystemWithMigrations(
                AppDataDirectory,
                CacheDirectory,
                FileSystem2.BaseDirectory.AppDataDirectory,
                FileSystem2.BaseDirectory.CacheDirectory);
        }

        static string AppDataDirectory
        {
            get
            {
                var value = Environment.GetEnvironmentVariable(XDG_DATA_HOME);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Path.Combine(value, Constants.HARDCODED_APP_NAME);
                }
                value = Environment.GetEnvironmentVariable(HOME);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Path.Combine(value, ".local", "share", Constants.HARDCODED_APP_NAME);
                }
                throw new PlatformNotSupportedException();
            }
        }

        static string CacheDirectory
        {
            get
            {
                var value = Environment.GetEnvironmentVariable(XDG_CACHE_HOME);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Path.Combine(value, Constants.HARDCODED_APP_NAME);
                }
                value = Environment.GetEnvironmentVariable(HOME);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return Path.Combine(value, ".cache", Constants.HARDCODED_APP_NAME);
                }
                throw new PlatformNotSupportedException();
            }
        }

        const string HOME = "HOME";

        const string XDG_DATA_HOME = "XDG_DATA_HOME";

        const string XDG_CACHE_HOME = "XDG_CACHE_HOME";
    }
}

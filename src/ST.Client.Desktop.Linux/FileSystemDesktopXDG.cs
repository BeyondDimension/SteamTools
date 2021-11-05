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

        /// <inheritdoc cref="FileSystemDesktop.InitFileSystem"/>
        public static void InitFileSystem()
        {
            var appDataPath = AppDataDirectory;
            var cachePath = CacheDirectory;

            var paths = new[] { appDataPath, cachePath, };
            var dict_paths = paths.ToDictionary(x => x, Directory.Exists);

            if (dict_paths.Values.All(x => !x))
            {
                var old_paths = new[] { FileSystemDesktop.AppDataDirectory, FileSystemDesktop.CacheDirectory, };
                if (old_paths.All(Directory.Exists)) // 迁移之前根目录上的文件夹
                {
                    for (int i = 0; i < old_paths.Length; i++)
                    {
                        var path = paths[i];
                        var old_path = old_paths[i];
                        Directory.Move(old_path, path);
                        dict_paths[path] = true;
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
            string GetAppDataDirectory() => appDataPath;
            string GetCacheDirectory() => cachePath;
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

        const string HOME = "$HOME";

        const string XDG_DATA_HOME = "$XDG_DATA_HOME";

        const string XDG_CACHE_HOME = "$XDG_CACHE_HOME";
    }
}

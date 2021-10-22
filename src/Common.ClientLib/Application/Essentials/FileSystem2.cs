using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    public sealed class FileSystem2 : IOPath.FileSystemBase
    {
        private FileSystem2() => throw new NotSupportedException();

        /// <summary>
        /// 初始化文件系统
        /// </summary>
        public static void InitFileSystem()
        {
            if (Essentials.IsSupported)
            {
                InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
                string GetAppDataDirectory() => FileSystem.AppDataDirectory;
                string GetCacheDirectory() => FileSystem.CacheDirectory;
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

#if DEBUG
        [Obsolete("use IOPath.AppDataDirectory", true)]
        public static string AppDataDirectory => IOPath.AppDataDirectory;

        [Obsolete("use IOPath.CacheDirectory", true)]
        public static string CacheDirectory => IOPath.CacheDirectory;
#endif
    }
}

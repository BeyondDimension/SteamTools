#if MONO_MAC
using MonoMac.Foundation;
#elif XAMARIN_MAC
using Foundation;
#endif
using System.IO;
using System.Properties;

namespace System.Application
{
    /// <inheritdoc cref="FileSystemDesktop"/>
    public sealed class FileSystemDesktopMac : IOPath.FileSystemBase
    {
        private FileSystemDesktopMac() => throw new NotSupportedException();

        /// <inheritdoc cref="FileSystemDesktop.InitFileSystem"/>
        public static void InitFileSystem()
        {
            // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/FileSystem/FileSystem.ios.tvos.watchos.macos.cs
            var appDataPath = Path.Combine(GetDirectory(NSSearchPathDirectory.LibraryDirectory), Constants.HARDCODED_APP_NAME);
            var cachePath = Path.Combine(GetDirectory(NSSearchPathDirectory.CachesDirectory), Constants.HARDCODED_APP_NAME);
            IOPath.DirCreateByNotExists(appDataPath);
            IOPath.DirCreateByNotExists(cachePath);
            InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
            string GetAppDataDirectory() => appDataPath;
            string GetCacheDirectory() => cachePath;
        }

        static string GetDirectory(NSSearchPathDirectory directory)
        {
            var dirs = NSSearchPath.GetDirectories(directory, NSSearchPathDomain.User);
            if (dirs == null || dirs.Length == 0)
            {
                // this should never happen...
                throw new NotSupportedException("this should never happen...");
                //return null;
            }
            return dirs[0];
        }
    }
}
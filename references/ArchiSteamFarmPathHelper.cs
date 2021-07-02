using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ArchiSteamFarm
{
    public static class ASFPathHelper
    {
        static readonly Lazy<string> _AppDataDirectory = new(() =>
        {
            var path = Path.Combine(IOPath.AppDataDirectory, "ASF");
            IOPath.DirCreateByNotExists(path);
            return path;
        });

        /// <summary>
        /// <list type="bullet">
        ///     <item>
        ///         AppData\ASF\config
        ///     </item>
        ///     <item>
        ///         AppData\ASF\plugins
        ///     </item>
        /// </list>
        /// </summary>
        public static string AppDataDirectory => _AppDataDirectory.Value;

        /// <summary>
        /// <list type="bullet">
        ///     <item>
        ///         Windows：Logs\ASF
        ///     </item>
        ///     <item>
        ///         Windows Desktop Bridge：{CacheDirectory}\Logs\ASF
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="logDirPath"></param>
        /// <returns></returns>
        public static string GetLogDirectory([NotNull] string logDirPath)
        {
            var path = (logDirPath.EndsWith(Path.DirectorySeparatorChar) ? logDirPath : logDirPath + Path.DirectorySeparatorChar) + "ASF";
            IOPath.DirCreateByNotExists(path);
            return path;
        }

        public static string GetNLogArchiveFileName([NotNull] string logDirectory)
        {
            var path = logDirectory + Path.DirectorySeparatorChar + SharedInfo.ArchivalLogFile;
            return path;
        }

        public static string GetNLogFileName([NotNull] string logDirectory)
        {
            var path = logDirectory + Path.DirectorySeparatorChar + SharedInfo.LogFile;
            return path;
        }

        static readonly Lazy<string> _WebsiteDirectory = new(() =>
        {
            var path = Path.Combine(IOPath.BaseDirectory, "ASF-ui");
            IOPath.DirCreateByNotExists(path);
            return path;
        });

        /// <summary>
        /// <list type="bullet">
        ///     <item>
        ///         Desktop：ASF-ui
        ///     </item>
        /// </list>
        /// </summary>
        public static string WebsiteDirectory => _WebsiteDirectory.Value;

#if DEBUG
        [Obsolete("use AppDataDirectory")]
        public static string HomeDirectory => AppDataDirectory;
#endif

        public const string NLogGeneralLayout = NLog.Logging.GeneralLayout;
    }
}
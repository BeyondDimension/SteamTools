using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System.Application.UI;
using System.IO;
using NLogLevel = NLog.LogLevel;

#if CONSOLEAPP
namespace System.Application
#else
namespace System.Application.UI
#endif
{
    static partial class Program
    {
        public static Logger? logger;

        public static string InitLogDir(string? alias = null)
        {
            var logDirPath = Path.Combine(
#if WINDOWS_DESKTOP_BRIDGE
                IOPath.CacheDirectory
#else
                AppContext.BaseDirectory
#endif
                , "Logs");
            IOPath.DirCreateByNotExists(logDirPath);
#if StartupTrace
            StartupTrace.Restart("InitLogDir.IO");
#endif
            var logDirPath_ = logDirPath + Path.DirectorySeparatorChar;

            InternalLogger.LogFile = logDirPath_ + "internal-nlog" + alias + ".txt";
            InternalLogger.LogLevel = NLogLevel.Error;
            var objConfig = new LoggingConfiguration();
            var logfile = new FileTarget("logfile")
            {
                FileName = logDirPath_ + "nlog-all-${shortdate}" + alias + ".log",
                Layout = "${longdate}|${level}|${logger}|${message} |${all-event-properties} ${exception:format=tostring}",
                ArchiveAboveSize = 10485760,
                MaxArchiveFiles = 14,
                MaxArchiveDays = 7,
            };
            objConfig.AddTarget(logfile);
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, logfile, "Microsoft.*");
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, logfile, "System.Net.Http.*");
            objConfig.AddRule(AppHelper.DefaultNLoggerMinLevel, NLogLevel.Fatal, logfile, "*");
#if StartupTrace
            StartupTrace.Restart("InitLogDir.CreateLoggingConfiguration");
#endif
            LogManager.Configuration = objConfig;

            return logDirPath;
        }
    }
}
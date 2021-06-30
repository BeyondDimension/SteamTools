using ArchiSteamFarm;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Filters;
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
#if WINDOWS_DESKTOP_BRIDGE || MAC
                IOPath.CacheDirectory
#else
                IOPath.BaseDirectory
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
            var defMinLevel = AppHelper.DefaultNLoggerMinLevel;
            var logfile = new FileTarget("logfile")
            {
                FileName = logDirPath_ + "nlog-all-${shortdate}" + alias + ".log",
                Layout = "${longdate}|${level}|${logger}|${message} |${all-event-properties} ${exception:format=tostring}",
                ArchiveAboveSize = 10485760,
                MaxArchiveFiles = 14,
                MaxArchiveDays = 7,
            };
            var asfLogDirPath = ASFPathHelper.GetLogDirectory(logDirPath_);
            FileTarget logfile_asf = new("File")
            {
                ArchiveFileName = ASFPathHelper.GetNLogArchiveFileName(asfLogDirPath),
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                ArchiveOldFileOnStartup = true,
                CleanupFileName = false,
                ConcurrentWrites = false,
                DeleteOldFileOnStartup = true,
                FileName = ASFPathHelper.GetNLogFileName(asfLogDirPath),
                Layout = ASFPathHelper.NLogGeneralLayout,
                ArchiveAboveSize = 10485760,
                MaxArchiveFiles = 10,
                MaxArchiveDays = 7,
            };
            objConfig.AddTarget(logfile);
            objConfig.AddTarget(logfile_asf);

            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, logfile, "Microsoft.*");
            objConfig.AddRule(NLogLevel.Error, NLogLevel.Fatal, logfile, "System.Net.Http.*");
            // https://github.com/NLog/NLog/wiki/When-Filter
            // https://github.com/NLog/NLog/wiki/Filtering-log-messages
            var rule = new LoggingRule("*", defMinLevel, NLogLevel.Fatal, logfile);
            rule.Filters.Add(new ConditionBasedFilter()
            {
                Condition = "starts-with(logger, 'ArchiSteamFarm')",
                Action = FilterResult.IgnoreFinal,
            });
            objConfig.LoggingRules.Add(rule);
            rule = new LoggingRule("*", defMinLevel, NLogLevel.Fatal, logfile_asf);
            rule.Filters.Add(new ConditionBasedFilter()
            {
                Condition = "not starts-with(logger, 'ArchiSteamFarm')",
                Action = FilterResult.IgnoreFinal,
            });
            objConfig.LoggingRules.Add(rule);
#if StartupTrace
            StartupTrace.Restart("InitLogDir.CreateLoggingConfiguration");
#endif
            LogManager.Configuration = objConfig;

            return logDirPath;
        }
    }
}
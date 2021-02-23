using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Properties;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NInternalLogger = NLog.Common.InternalLogger;
using NLogLevel = NLog.LogLevel;
using NLogManager = NLog.LogManager;

namespace System.Application.UI
{
    public static class AppHelper
    {
        static AppHelper()
        {
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (mainModule == null)
                throw new ArgumentNullException(nameof(mainModule));
            var fullName = mainModule.FileName;
            if (fullName == null)
                throw new ArgumentNullException(nameof(fullName));
            var programName = Path.GetFileName(fullName);
            if (programName == null)
                throw new ArgumentNullException(nameof(programName));
            ProgramName = programName;
        }

        /// <summary>
        /// 获取当前主程序文件名，例如word.exe
        /// </summary>
        public static string ProgramName { get; }

        #region Logger

        const LogLevel DefaultLoggerMinLevel = ThisAssembly.Debuggable ? LogLevel.Debug : LogLevel.Error;

        /// <summary>
        /// Convert log level to NLog variant.
        /// </summary>
        /// <param name="logLevel">level to be converted.</param>
        /// <returns></returns>
        static NLogLevel ConvertLogLevel(LogLevel logLevel) => logLevel switch
        {
            // https://github.com/NLog/NLog.Extensions.Logging/blob/v1.7.0/src/NLog.Extensions.Logging/Logging/NLogLogger.cs#L416
            LogLevel.Trace => NLogLevel.Trace,
            LogLevel.Debug => NLogLevel.Debug,
            LogLevel.Information => NLogLevel.Info,
            LogLevel.Warning => NLogLevel.Warn,
            LogLevel.Error => NLogLevel.Error,
            LogLevel.Critical => NLogLevel.Fatal,
            LogLevel.None => NLogLevel.Off,
            _ => NLogLevel.Debug,
        };

        static void SetNLoggerMinLevel(LogLevel logLevel) => SetNLoggerMinLevel(ConvertLogLevel(logLevel));

        static void SetNLoggerMinLevel(NLogLevel logLevel)
        {
            NLogManager.GlobalThreshold = logLevel;
            NInternalLogger.LogLevel = logLevel;
        }

        /// <summary>
        /// 日志过滤选项
        /// </summary>
        static LoggerFilterOptions LoggerFilterOptions => DI.Get<IOptions<LoggerFilterOptions>>().Value;

        internal static void Configure(ILoggingBuilder builder)
        {
            var logLevel = DefaultLoggerMinLevel;
            builder.SetMinimumLevel(logLevel);
            SetNLoggerMinLevel(logLevel);

            // 可以多个日志提供同时用，比如还可以在 Win 平台再添加一个 Windows 事件日志

            builder.AddNLog(); // 添加 NLog 日志
        }

        /// <summary>
        /// 日志最低等级
        /// </summary>
        public static LogLevel LoggerMinLevel
        {
            get => LoggerFilterOptions.MinLevel;
            set
            {
                LoggerFilterOptions.MinLevel = value;
                SetNLoggerMinLevel(value);
            }
        }

        /// <summary>
        /// 启用日志
        /// </summary>
        public static bool EnableLogger
        {
            get => LoggerMinLevel < LogLevel.None;
            set
            {
                LoggerMinLevel = value ? DefaultLoggerMinLevel : LogLevel.None;
            }
        }

        #endregion

        /// <summary>
        /// 当前运行程序是否为官方渠道包
        /// </summary>
        public static bool IsOfficialChannelPackage
        {
            get
            {
                var pk = typeof(AppHelper).Assembly.GetName().GetPublicKey();
                if (pk == null) return false;
                var pkStr = ", PublicKey=" + string.Join(string.Empty, pk.Select(x => x.ToString("x2")));
                return pkStr == ThisAssembly.PublicKey;
            }
        }

#if DEBUG

        [Obsolete("use EnableLogger", true)]
        public static bool EnableTextLog
        {
            get => EnableLogger;
            set => EnableLogger = value;
        }

#endif
    }
}
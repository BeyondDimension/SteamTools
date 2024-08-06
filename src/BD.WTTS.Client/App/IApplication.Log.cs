// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    #region Logger

    [Mobius(
"""
Mobius.Helpers.LogInitHelper
""")]
    const LogLevel DefaultLoggerMinLevel = IPCSubProcessFileSystem.DefaultLoggerMinLevel;

    [Mobius(
"""
Mobius.Helpers.LogInitHelper
""")]
    public static NLogLevel DefaultNLoggerMinLevel => IPCSubProcessFileSystem.DefaultNLoggerMinLevel;

    /// <summary>
    /// Convert log level to NLog variant.
    /// </summary>
    /// <param name="logLevel">level to be converted.</param>
    /// <returns></returns>
    [Mobius(
"""
Mobius.Helpers.LogInitHelper
""")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static NLogLevel ConvertLogLevel(LogLevel logLevel) => IPCSubProcessFileSystem.ConvertLogLevel(logLevel);

    [Mobius(
"""
Mobius.Helpers.LogInitHelper
""")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SetNLoggerMinLevel(LogLevel logLevel) => SetNLoggerMinLevel(ConvertLogLevel(logLevel));

    [Mobius(
"""
Mobius.Helpers.LogInitHelper
""")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void SetNLoggerMinLevel(NLogLevel logLevel)
    {
        NLogManager.GlobalThreshold = logLevel;
        NInternalLogger.LogLevel = logLevel;
    }

    [Mobius(Obsolete = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TrySetLoggerMinLevel(LogLevel logLevel)
    {
        try
        {
            var o = LoggerFilterOptions;
            if (o != null) o.MinLevel = logLevel;
        }
        catch
        {
        }
        SetNLoggerMinLevel(ConvertLogLevel(logLevel));
    }

    [Mobius(
"""
Mobius.Helpers.LogInitHelper
""")]
    private static LoggerFilterOptions? _LoggerFilterOptions;

    /// <summary>
    /// 日志过滤选项
    /// </summary>
    [Mobius(
"""
Mobius.Helpers.LogInitHelper
""")]
    static LoggerFilterOptions? LoggerFilterOptions
    {
        get
        {
            if (_LoggerFilterOptions != null) return _LoggerFilterOptions;
            return Ioc.Get_Nullable<IOptions<LoggerFilterOptions>>()?.Value;
        }
        set => _LoggerFilterOptions = value;
    }

    /// <summary>
    /// 配置 NLog 提供程序，仅初始化时调用
    /// </summary>
    [Mobius(
"""
Mobius.Helpers.LogInitHelper.GetMobiusLoggingBuilder
""")]
    public static Action<ILoggingBuilder> ConfigureLogging(LogLevel minLevel = DefaultLoggerMinLevel)
    {
        return (ILoggingBuilder builder) =>
        {
            builder.ClearProviders();

            //builder.SetMinimumLevel(minLevel);
            //SetNLoggerMinLevel(minLevel);

            builder.AddNLog(NLogManager.Configuration); // 添加 NLog 日志
#if ((WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)) && DEBUG
            builder.AddConsole(); // 添加控制台日志
#endif
            //#if WINDOWS
            //            builder.AddEventLog(new Microsoft.Extensions.Logging.EventLog.EventLogSettings()
            //            {
            //                SourceName = Constants.HARDCODED_APP_NAME,
            //            }); // 添加 Windows 事件日志
            //#endif
            //#if ANDROID
            //#if DEBUG
            //            // Android Logcat Provider Impl
            //            //builder.AddProvider(PlatformLoggerProvider.Instance);
            //#endif
            //#elif MACOS || MACCATALYST || (IOS && DEBUG)
            //            builder.AddProvider(Logging.OSLogLoggerProvider.Instance);
            //#endif
        };
    }

    /// <summary>
    /// 日志最低等级
    /// </summary>
    [Mobius(
"""
Mobius.Helpers.LogInitHelper
""")]
    public static LogLevel LoggerMinLevel
    {
        get
        {
            var o = LoggerFilterOptions;
            if (o == null)
            {
                o = new();
                LoggerFilterOptions = o;
            }
            return o.MinLevel;
        }

        set
        {
            var o = LoggerFilterOptions;
            if (o != null)
            {
                o.MinLevel = value;
            }
            SetNLoggerMinLevel(value);
        }
    }

    /// <summary>
    /// 启用日志
    /// </summary>
    [Mobius(Obsolete = true)]
    public static bool EnableLogger
    {
        get => LoggerMinLevel > LogLevel.None;
        set
        {
            LoggerMinLevel = value ? DefaultLoggerMinLevel : LogLevel.None;
        }
    }

    #endregion

    #region LogHelper

    /// <summary>
    /// 日志存放文件夹路径
    /// </summary>
    [Mobius(
"""
MobiusHost.LogDirPath
""")]
    static string LogDirPath { get; internal set; } = string.Empty;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// 日志存放文件夹路径(ASF)
    /// </summary>
    [Mobius(Obsolete = true)]
    static string LogDirPathASF { get; set; } = string.Empty;

#endif

#if DEBUG
    /// <summary>
    /// 日志文件夹是否存放在缓存文件夹中，通常日志文件夹将存放在基目录上，因某些平台基目录为只读，则只能放在缓存文件夹中
    /// </summary>
    [Obsolete("only True.", true)]
    static bool LogUnderCache => true;
#endif

    [Mobius(Obsolete = true)]
    static Action? ASFInitCoreLoggers { get; private set; }

    #endregion
}
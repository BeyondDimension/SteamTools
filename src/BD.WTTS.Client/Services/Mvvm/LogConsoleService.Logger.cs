#if !ANDROID && !IOS
using Utf8StringInterpolation;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial class LogConsoleService
{
    /// <summary>
    /// 控制台显示最大字符数
    /// </summary>
    const int ConsoleMaxCharCount = 10_0000;

    static readonly int ConsoleMaxByteCount = Encoding.UTF8.GetMaxByteCount(ConsoleMaxCharCount);

    /// <summary>
    /// 日志源
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    internal sealed partial class Source
    {
        List<byte[]>? chunk;
        (string? str, long byteLength)? message = null;

#if DEBUG
        readonly string moduleName;

        public Source(string moduleName)
        {
            this.moduleName = moduleName;
        }

        string DebuggerDisplay() =>
$"""
moduleName: {moduleName}
ByteLength: {ByteLength}
{ToString()}
""";
#endif

        public long ByteLength { get; private set; }

        public void BuilderAppend(byte[] value)
        {
            (chunk ??= new()).Add(value);
            ByteLength += value.Length;
        }

        void BuilderClear()
        {
            if (chunk != null && chunk.Count > 1)
            {
                // 保留最后 21.3% 的日志
                var keppLength = (int)MathF.Floor(chunk.Count * .213f);
                if (keppLength <= 0 || keppLength >= chunk.Count)
                {
                    keppLength = 1;
                }
                chunk = chunk.TakeLast(keppLength).ToList();
            }

            message = null;
            ByteLength = 0;
        }

        /// <summary>
        /// 追加收到的日志 UTF-8 字节
        /// </summary>
        public void Append(byte[]? value)
        {
            if (value != null && value.Length > 0)
            {
                if (ByteLength > ConsoleMaxByteCount)
                {
                    BuilderClear();
                }

                // 追加收到的日志 UTF-8 字节
                BuilderAppend(value);
            }
        }

        public override string? ToString()
        {
            if (message != null)
            {
                if (ByteLength == message.Value.byteLength)
                {
                    // 根据 len 判断是否使用缓存
                    return message.Value.str;
                }
            }

            if (chunk == null)
            {
                return null;
            }
            else if (chunk.Count <= 0)
            {
                return string.Empty;
            }

            using var buffer = Utf8String.CreateWriter(out var writer);

            foreach (var it in chunk)
            {
                // 拼接字符串
                writer.AppendUtf8(it);
            }

            writer.Flush();

            var u8_bytes = buffer.ToArray();

            chunk = [u8_bytes];

            var result = Encoding.UTF8.GetString(u8_bytes);

            // 设置缓存值
            message = new(result, ByteLength);
            return result;
        }
    }

    #region 服务进程部分

    static readonly string _messagePadding = new string(' ', 6);
    static readonly string _newLineWithMessagePadding = Environment.NewLine + _messagePadding;

    static byte[] CreateDefaultLogMessage(string logName, int eventId, string? message, Exception? exception)
    {
        // https://github.com/dotnet/extensions/blob/v3.1.5/src/Logging/Logging.Console/src/ConsoleLogger.cs
        // TODO: 对比 SimpleConsoleFormatter 实现带 Color 的版本
        // https://github.com/dotnet/runtime/blob/v9.0.0-preview.7.24405.7/src/libraries/Microsoft.Extensions.Logging.Console/src/SimpleConsoleFormatter.cs

        if (!string.IsNullOrEmpty(message))
        {
            StringBuilder builder = new();
            CreateDefaultLogMessage(builder, logName, eventId, message, exception);

            var str = builder.ToString();
            return Encoding.UTF8.GetBytes(str);
        }
        else
        {
            using var buffer = Utf8String.CreateWriter(out var logBuilder);

            // Example:
            // INFO: ConsoleApp.Program[10]
            //       Request received

            // category and event id
            logBuilder.Append(logName);
            logBuilder.Append('[');
            logBuilder.Append(eventId.ToString());
            logBuilder.AppendLine("]");

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception != null)
                // exception message
                logBuilder.AppendLine(exception.ToString());

            logBuilder.Flush();

            var result = buffer.ToArray();
            return result;
        }
    }

    static void CreateDefaultLogMessage(StringBuilder logBuilder, string logName, int eventId, string? message, Exception? exception)
    {
        // Example:
        // INFO: ConsoleApp.Program[10]
        //       Request received

        // category and event id
        logBuilder.Append(logName);
        logBuilder.Append('[');
        logBuilder.Append(eventId);
        logBuilder.AppendLine("]");

        if (!string.IsNullOrEmpty(message))
        {
            // message
            logBuilder.Append(_messagePadding);

            var len = logBuilder.Length;
            logBuilder.AppendLine(message);
            logBuilder.Replace(Environment.NewLine, _newLineWithMessagePadding, len, message.Length);
        }

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (exception != null)
            // exception message
            logBuilder.AppendLine(exception.ToString());
    }

    static byte[] CreateDefaultLogMessageWithNLogFileLayout(LogLevel logLevel, string logName, int eventId, string? message, Exception? exception)
    {
        // Layout = "
        // ${longdate}|
        // ${level}|
        // ${logger}|${message} |${all-event-properties} ${exception:format=tostring}",

        using var buffer = Utf8String.CreateWriter(out var logBuilder);

        logBuilder.Append(DateTime.Now.ToString(
            "yyyy-MM-dd HH:mm:ss.fff",
            CultureInfo.InvariantCulture));
        logBuilder.AppendUtf8("|"u8);

        switch (logLevel) // see NLog.LogLevel
        {
            case LogLevel.Trace:
                logBuilder.AppendUtf8("Trace"u8);
                break;
            case LogLevel.Debug:
                logBuilder.AppendUtf8("Debug"u8);
                break;
            case LogLevel.Information:
                logBuilder.AppendUtf8("Info"u8);
                break;
            case LogLevel.Warning:
                logBuilder.AppendUtf8("Warn"u8);
                break;
            case LogLevel.Error:
                logBuilder.AppendUtf8("Error"u8);
                break;
            case LogLevel.Critical:
                logBuilder.AppendUtf8("Fatal"u8);
                break;
            case LogLevel.None:
                logBuilder.AppendUtf8("Off"u8);
                break;
            default:
                logBuilder.Append(logLevel.ToString());
                break;
        }
        logBuilder.AppendUtf8("|"u8);

        logBuilder.Append(logName);
        logBuilder.AppendUtf8("|"u8);

        logBuilder.Append(message);
        logBuilder.AppendUtf8("| "u8);

        logBuilder.Append(exception?.ToString());
        logBuilder.AppendLine();

        logBuilder.Flush();

        var result = buffer.ToArray();
        return result;
    }

#if APP_REVERSE_PROXY
    internal static readonly Source Builder = new(
#if DEBUG
        AssemblyInfo.Accelerator
#endif
        );
#endif

    internal sealed class Utf8StringLogger(string name) : ILogger
    {
        readonly string name = name;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // TODO: 可选加入设置项
#if DEBUG
            return true;
#else
            return logLevel >= LogLevel.Information;
#endif
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {

#if APP_REVERSE_PROXY
                var logMessageU8 = CreateDefaultLogMessageWithNLogFileLayout(logLevel, name, eventId.Id, message, exception);
                Builder.Append(logMessageU8);
#else
                try
                {
#if APP_REVERSE_PROXY
                    //var s = IReverseProxyService.Instance;
                    // TODO Ipc
#else
                    var s = IPCMainProcessService.Instance;
#endif
                    var logMessage = CreateDefaultLogMessage(name, eventId.Id, message, exception);
                    s.WriteMessage(Utf8StringLoggerProvider.ModuleName, logMessage);
                }
                catch
                {
                }
#endif
            }
        }
    }

    /// <summary>
    /// An empty scope without any logic
    /// <para>https://github.com/dotnet/extensions/blob/v3.1.5/src/Logging/shared/NullScope.cs</para>
    /// <para>https://github.com/dotnet/runtime/blob/v5.0.0-rtm.20519.4/src/libraries/Common/src/Extensions/Logging/NullScope.cs</para>
    /// </summary>
    sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        NullScope()
        {
        }

        public void Dispose()
        {
        }
    }

    internal sealed class Utf8StringLoggerProvider : ILoggerProvider
    {
        internal static string? ModuleName { get; private set; }

        public Utf8StringLoggerProvider(string moduleName)
        {
            ModuleName = moduleName;
        }

        public ILogger CreateLogger(string name)
        {
            return new Utf8StringLogger(name);
        }

        void IDisposable.Dispose()
        {
        }
    }

    #endregion
}
#endif

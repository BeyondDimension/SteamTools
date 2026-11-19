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
    [DebuggerDisplay("Log bytes = {length}")]
    internal sealed partial class Source
    {
        readonly object sync = new();
        byte[] buffer = [];
        int length;
        string? snapshot;

        public void Append(byte[]? value)
        {
            if (value is not { Length: > 0 })
                return;

            lock (sync)
            {
                var previousLength = length;
                ReadOnlySpan<byte> entry = value;
                if (entry.Length > ConsoleMaxByteCount)
                {
                    length = 0;
                    entry = entry[^ConsoleMaxByteCount..];
                    entry = entry[Utf8StartOffset(entry)..];
                }

                if (length > ConsoleMaxByteCount - entry.Length)
                {
                    // Leave room for new entries and keep recent complete log lines.
                    var keep = Math.Min(ConsoleMaxByteCount / 5, ConsoleMaxByteCount - entry.Length);
                    var tail = buffer.AsSpan(length - keep, keep);
                    if (keep > 0 && buffer[length - keep - 1] != (byte)'\n')
                    {
                        var newline = tail.IndexOf((byte)'\n');
                        var start = newline >= 0 ? newline + 1 : Utf8StartOffset(tail);
                        tail = tail[start..];
                    }
                    tail.CopyTo(buffer);
                    length = tail.Length;
                }

                var required = length + entry.Length;
                if (required > buffer.Length)
                {
                    var grownCapacity = Math.Max(256, buffer.Length * 2);
                    Array.Resize(ref buffer, Math.Clamp(grownCapacity, required, ConsoleMaxByteCount));
                }

                entry.CopyTo(buffer.AsSpan(length));
                if (required < previousLength)
                    buffer.AsSpan(required, previousLength - required).Clear();
                length = required;
                snapshot = null;
            }
        }

        static int Utf8StartOffset(ReadOnlySpan<byte> bytes)
        {
            // A cut through a multi-byte character must skip its continuation bytes.
            var index = 0;
            while (index < bytes.Length && (bytes[index] & 0xc0) == 0x80)
                index++;
            return index;
        }

        public override string? ToString()
        {
            lock (sync)
                return length == 0 ? null : snapshot ??= Encoding.UTF8.GetString(buffer, 0, length);
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
    internal static readonly Source Builder = new();
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

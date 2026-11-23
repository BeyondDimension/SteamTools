#if !ANDROID && !IOS
#if APP_REVERSE_PROXY
using System.Buffers;
using System.Text.Unicode;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial class LogConsoleService
{
    /// <summary>
    /// 日志源
    /// </summary>
    [DebuggerDisplay("Log bytes = {length}")]
    internal sealed partial class Source
    {
        internal const int MaxBytes = 256 * 1024;

        readonly object sync = new();
        byte[] buffer = [];
        int length;
        long end;
        string? snapshot;

        public void Append(ReadOnlySpan<byte> entry)
        {
            if (entry.IsEmpty) return;

            lock (sync)
            {
                end += entry.Length;
                if (entry.Length > MaxBytes)
                {
                    length = 0;
                    entry = entry[^MaxBytes..];
                    entry = entry[Utf8StartOffset(entry)..];
                }

                if (length > MaxBytes - entry.Length)
                {
                    // Leave room for new entries and keep recent complete log lines.
                    var keep = Math.Min(MaxBytes / 5, MaxBytes - entry.Length);
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
                    Array.Resize(ref buffer, Math.Clamp(grownCapacity, required, MaxBytes));
                }

                entry.CopyTo(buffer.AsSpan(length));
                length = required;
                snapshot = null;
            }
        }

        internal (long End, bool Reset, byte[] Utf8) ReadSince(long offset)
        {
            lock (sync)
            {
                var first = end - length;
                var reset = offset < first || offset > end;
                var start = reset ? 0 : (int)(offset - first);
                return (end, reset, start == length ? [] : buffer.AsSpan(start, length - start).ToArray());
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

        StringBuilder builder = new();
        CreateDefaultLogMessage(builder, logName, eventId, message, exception);
        return Encoding.UTF8.GetBytes(builder.ToString());
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

#if APP_REVERSE_PROXY
    static void CaptureProxyLog(LogLevel logLevel, string logName, string? message, Exception? exception)
    {
        // Layout = "
        // ${longdate}|
        // ${level}|
        // ${logger}|${message} |${all-event-properties} ${exception:format=tostring}",

        var level = logLevel switch // see NLog.LogLevel
        {
            LogLevel.Trace => "Trace",
            LogLevel.Debug => "Debug",
            LogLevel.Information => "Info",
            LogLevel.Warning => "Warn",
            LogLevel.Error => "Error",
            LogLevel.Critical => "Fatal",
            LogLevel.None => "Off",
            _ => logLevel.ToString(),
        };

        // Cap the preview before encoding to bound the temporary buffer.
        const int maxEntryCharacters = 32 * 1024;
        var category = Tail(logName, 128);
        var detail = Tail(exception?.ToString(), maxEntryCharacters);
        var preview = Tail(message, maxEntryCharacters - detail.Length);
        var characterCount = "yyyy-MM-dd HH:mm:ss.fff".Length + "|||| ".Length + Environment.NewLine.Length
            + level.Length + category.Length + preview.Length + detail.Length;
        var capacity = Encoding.UTF8.GetMaxByteCount(characterCount);
        byte[]? rented = null;
        Span<byte> entry = capacity <= 512 ? stackalloc byte[capacity] : (rented = ArrayPool<byte>.Shared.Rent(capacity));
        try
        {
            if (Utf8.TryWrite(entry, CultureInfo.InvariantCulture,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}|{level}|{category}|{preview}| {detail}{Environment.NewLine}", out var written))
            {
                Builder.Append(entry[..written]);
            }
        }
        finally
        {
            if (rented != null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    static ReadOnlySpan<char> Tail(string? text, int maxCharacters)
    {
        var span = text.AsSpan();
        if (maxCharacters == 0) return [];
        if (span.Length <= maxCharacters) return span;
        var start = span.Length - maxCharacters;
        return span[(char.IsLowSurrogate(span[start]) ? start + 1 : start)..];
    }
#endif

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
                CaptureProxyLog(logLevel, name, message, exception);
#else
                try
                {
                    var s = IPCMainProcessService.Instance;
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

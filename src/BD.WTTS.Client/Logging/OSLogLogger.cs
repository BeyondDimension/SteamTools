#if MACOS || MACCATALYST || IOS
// https://github.com/unoplatform/uno.extensions.logging/blob/1.4.0/src/Uno.Extensions.Logging.OSLog/OSLogLogger.cs
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Logging;

internal sealed class OSLogLogger : ILogger<object>, ILogger
{
    static readonly string _loglevelPadding = ": ";
    static readonly string _messagePadding;
    static readonly string _newLineWithMessagePadding;
    static readonly StringBuilder _logBuilder = new();

    readonly string _name;

    static OSLogLogger()
    {
        var logLevelString = GetLogLevelString(LogLevel.Information);
        _messagePadding = new string(' ', logLevelString.Length + _loglevelPadding.Length);
        _newLineWithMessagePadding = Environment.NewLine + _messagePadding;
    }

    public OSLogLogger()
        : this(string.Empty)
    {
    }

    public OSLogLogger(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);

        if (!string.IsNullOrEmpty(message) || exception != null)
        {
            WriteMessage(logLevel, _name, eventId.Id, message, exception);
        }
    }

    static void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception? exception)
    {
        lock (_logBuilder)
        {
            try
            {
                CreateDefaultLogMessage(_logBuilder, logLevel, logName, eventId, message, exception);
                var formattedMessage = _logBuilder.ToString();

                var osLogLevel = logLevel switch
                {
                    LogLevel.Critical => CoreFoundation.OSLogLevel.Fault,
                    LogLevel.Error => CoreFoundation.OSLogLevel.Info,
                    LogLevel.Warning => CoreFoundation.OSLogLevel.Info,
                    LogLevel.Information => CoreFoundation.OSLogLevel.Default,
                    LogLevel.Debug => CoreFoundation.OSLogLevel.Debug,
                    LogLevel.Trace => CoreFoundation.OSLogLevel.Debug,
                    _ => CoreFoundation.OSLogLevel.Default,
                };

                CoreFoundation.OSLog.Default.Log(osLogLevel, formattedMessage);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to log \"{message}\": {ex}");
            }
            finally
            {
                _logBuilder.Clear();
            }
        }
    }

    static void CreateDefaultLogMessage(StringBuilder logBuilder, LogLevel logLevel, string logName, int eventId, string message, Exception? exception)
    {
        logBuilder.Append(GetLogLevelString(logLevel));
        logBuilder.Append(_loglevelPadding);
        logBuilder.Append(logName);
        logBuilder.Append('[');
        logBuilder.Append(eventId);
        logBuilder.Append("] ");

        if (!string.IsNullOrEmpty(message))
        {
            var len = logBuilder.Length;
            logBuilder.Append(message);
            logBuilder.Replace(Environment.NewLine, _newLineWithMessagePadding, len, message.Length);
        }

        if (exception != null)
        {
            // exception message
            logBuilder.AppendLine();
            logBuilder.Append(exception.ToString());
        }
    }

    static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
        };
    }
}
#endif
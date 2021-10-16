#if !MONO_MAC
// https://github.com/unoplatform/uno.extensions.logging/blob/master/src/Uno.Extensions.Logging.OSLog/OSLogLogger.cs
using System;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using OSLog = CoreFoundation.OSLog;

namespace Uno.Extensions.Logging
{
    internal class OSLogLogger : ILogger<object>, ILogger
    {
        private static readonly string _loglevelPadding = ": ";
        private static readonly string _messagePadding;
        private static readonly string _newLineWithMessagePadding;
        private static readonly StringBuilder _logBuilder = new StringBuilder();

        private readonly string _name;

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

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoOpDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
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

        private void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            lock (_logBuilder)
            {
                try
                {
                    CreateDefaultLogMessage(_logBuilder, logLevel, logName, eventId, message, exception);
                    var formattedMessage = _logBuilder.ToString();

                    var osLogLevel = logLevel switch
                    {
                        LogLevel.Critical => OSLogLevel.Fault,
                        LogLevel.Error => OSLogLevel.Info,
                        LogLevel.Warning => OSLogLevel.Info,
                        LogLevel.Information => OSLogLevel.Default,
                        LogLevel.Debug => OSLogLevel.Debug,
                        LogLevel.Trace => OSLogLevel.Debug,
                        _ => OSLogLevel.Default,
                    };

                    OSLog.Default.Log(osLogLevel, formattedMessage);
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

        private void CreateDefaultLogMessage(StringBuilder logBuilder, LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            logBuilder.Append(GetLogLevelString(logLevel));
            logBuilder.Append(_loglevelPadding);
            logBuilder.Append(logName);
            logBuilder.Append("[");
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

        private static string GetLogLevelString(LogLevel logLevel)
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

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance = new NoOpDisposable();

            public void Dispose() { }
        }
    }
}
#endif
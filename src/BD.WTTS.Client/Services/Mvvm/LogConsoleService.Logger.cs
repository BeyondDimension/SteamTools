#if !ANDROID && !IOS
using Utf8StringInterpolation;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial class LogConsoleService
{
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

#if APP_REVERSE_PROXY
        internal static readonly StringBuilder Builder = new();
#endif

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
                try
                {
                    if (Builder.Length >= 8000)
                    {
                        Builder.Remove(0, 8000);
                    }
                }
                catch
                {
                }
                CreateDefaultLogMessage(Builder, name, eventId.Id, message, exception);
#else
                try
                {
#if APP_REVERSE_PROXY
                    //var s = IReverseProxyService.Instance;
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

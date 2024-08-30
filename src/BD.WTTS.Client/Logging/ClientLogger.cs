// ReSharper disable once CheckNamespace
namespace BD.WTTS.Logging;

/// <summary>
/// 客户端日志
/// <para>https://github.com/dotnet/extensions/blob/v3.1.5/src/Logging/Logging.Console/src/ConsoleLogger.cs</para>
/// </summary>
public abstract class ClientLogger : ILogger
{
    static readonly string _messagePadding = new string(' ', 6);
    static readonly string _newLineWithMessagePadding = Environment.NewLine + _messagePadding;

    protected readonly string name;

    public ClientLogger(string name) => this.name = name;

    public virtual IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public virtual bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        var message = formatter(state, exception);

        if (!string.IsNullOrEmpty(message) || exception != null)
            WriteMessage(logLevel, name, eventId.Id, message, exception);
    }

    public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string? message, Exception? exception)
    {
        var logBuilder = new StringBuilder();

        CreateDefaultLogMessage(logBuilder, logName, eventId, message, exception);

        var logMessage = logBuilder.ToString();
        WriteMessage(logLevel, logMessage);
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

    public abstract void WriteMessage(LogLevel logLevel, string message);
}
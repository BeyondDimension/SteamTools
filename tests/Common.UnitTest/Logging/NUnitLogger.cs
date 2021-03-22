using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace System.Logging
{
    public class NUnitLogger : ClientLogger
    {
        public NUnitLogger(string name) : base(name)
        {
        }

        static string GetLogLevelString(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
        };

        public override void WriteMessage(LogLevel logLevel, string message)
        {
            var logLevelString = GetLogLevelString(logLevel);
            TestContext.Write(logLevelString);
            TestContext.Write(": ");
            TestContext.WriteLine(message);
        }
    }
}
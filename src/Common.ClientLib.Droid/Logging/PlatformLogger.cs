using Microsoft.Extensions.Logging;
using DroidLog = Android.Util.Log;

// ReSharper disable once CheckNamespace
namespace System.Logging
{
    /// <inheritdoc cref="ClientLogger"/>
    public class PlatformLogger : ClientLogger
    {
        readonly string tag;

        public PlatformLogger(string name) : base(name)
        {
            tag = LogHelper.GetDroidTag(name);
        }

        public override bool IsEnabled(LogLevel logLevel)
        {
            var priority = logLevel.ToLogPriority();
            var result = DroidLog.IsLoggable(tag, priority);
            return result;
        }

        public override void WriteMessage(LogLevel logLevel, string message)
        {
            var priority = logLevel.ToLogPriority();
            DroidLog.WriteLine(priority, tag, message);
        }
    }
}
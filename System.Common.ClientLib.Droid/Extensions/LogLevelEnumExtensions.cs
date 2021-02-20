using Android.Util;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Enum 扩展 <see cref="LogLevel"/>
    /// </summary>
    public static class LogLevelEnumExtensions
    {
        /// <summary>
        /// 将MS扩展日志等级(<see cref="LogLevel"/>)转换为安卓日志等级(<see cref="LogPriority"/>)
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static LogPriority ToLogPriority(this LogLevel level) => level switch
        {
            LogLevel.Trace => LogPriority.Verbose,
            LogLevel.Debug => LogPriority.Debug,
            LogLevel.Information => LogPriority.Info,
            LogLevel.Warning => LogPriority.Warn,
            LogLevel.Error => LogPriority.Error,
            LogLevel.Critical => LogPriority.Assert,
            _ => (LogPriority)int.MaxValue,
        };
    }
}
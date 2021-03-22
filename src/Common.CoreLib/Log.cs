using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace System
{
    /// <summary>
    /// 日志
    /// <para>使用说明：</para>
    /// <para>在类中定义 const string TAG = 类名(长度小于等于23)</para>
    /// <para>使用 Log.Debug(TAG,... / Log.Info(TAG,... / Log.Warn(TAG,... / Log.Error(TAG,...</para>
    /// </summary>
    public static class Log
    {
        internal static ILoggerFactory Factory => DI.Get<ILoggerFactory>();

        #region Debug

        [Conditional("DEBUG")]
        public static void Debug(string tag, string msg)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogDebug(msg);
        }

        [Conditional("DEBUG")]
        public static void Debug(string tag, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogDebug(msg, args);
        }

        [Conditional("DEBUG")]
        public static void Debug(string tag, Exception exception, string msg)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogDebug(exception, msg);
        }

        [Conditional("DEBUG")]
        public static void Debug(string tag, Exception exception, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogDebug(exception, msg, args);
        }

        [Conditional("DEBUG")]
        public static void Debug(string tag, EventId eventId, Exception exception, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogDebug(eventId, exception, msg, args);
        }

        [Conditional("DEBUG")]
        public static void Debug(string tag, EventId eventId, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogDebug(eventId, msg, args);
        }

        #endregion

        #region Error

        public static void Error(string tag, string msg)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogError(msg);
        }

        public static void Error(string tag, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogError(msg, args);
        }

        public static void Error(string tag, Exception exception, string msg)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogError(exception, msg);
        }

        public static void Error(string tag, Exception exception, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogError(exception, msg, args);
        }

        public static void Error(string tag, EventId eventId, Exception exception, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogError(eventId, exception, msg, args);
        }

        public static void Error(string tag, EventId eventId, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogError(eventId, msg, args);
        }

        #endregion

        #region Info

        public static void Info(string tag, string msg)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogInformation(msg);
        }

        public static void Info(string tag, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogInformation(msg, args);
        }

        public static void Info(string tag, Exception exception, string msg)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogInformation(exception, msg);
        }

        public static void Info(string tag, Exception exception, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogInformation(exception, msg, args);
        }

        public static void Info(string tag, EventId eventId, Exception exception, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogInformation(eventId, exception, msg, args);
        }

        public static void Info(string tag, EventId eventId, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogInformation(eventId, msg, args);
        }

        #endregion

        #region Warn

        public static void Warn(string tag, string msg)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogWarning(msg);
        }

        public static void Warn(string tag, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogWarning(msg, args);
        }

        public static void Warn(string tag, Exception exception, string msg)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogWarning(exception, msg);
        }

        public static void Warn(string tag, Exception exception, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogWarning(exception, msg, args);
        }

        public static void Warn(string tag, EventId eventId, Exception exception, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogWarning(eventId, exception, msg, args);
        }

        public static void Warn(string tag, EventId eventId, string msg, params object?[] args)
        {
            var logger = Factory.CreateLogger(tag);
            logger.LogWarning(eventId, msg, args);
        }

        #endregion
    }
}
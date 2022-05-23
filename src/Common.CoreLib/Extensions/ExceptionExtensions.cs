using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ExceptionExtensions
    {
        static bool JavaInterop => OperatingSystem2.IsAndroid();

        /// <summary>
        /// 获取异常是否为已知类型，通常不为 <see cref="ExceptionKnownType.Unknown"/> 的异常不需要纪录日志
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static ExceptionKnownType GetKnownType(this Exception e)
        {
            if (e is TaskCanceledException)
            {
                return ExceptionKnownType.TaskCanceled;
            }
            else if (e is OperationCanceledException)
            {
                return ExceptionKnownType.OperationCanceled;
            }
            if (JavaInterop)
            {
                var typeName = e.GetType().FullName;
                switch (typeName)
                {
                    case "Java.Security.Cert.CertificateNotYetValidException":
                        return ExceptionKnownType.CertificateNotYetValid;
                    case "Java.IO.IOException":
                        if (e.Message == "Canceled")
                        {
                            return ExceptionKnownType.Canceled;
                        }
                        break;
                }
            }
            e = e.InnerException;
            if (e != null)
            {
                return GetKnownType(e);
            }
            return ExceptionKnownType.Unknown;
        }

        /// <summary>
        /// 是否为取消操作异常
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsCanceledException(this ExceptionKnownType value)
            => value == ExceptionKnownType.Canceled ||
            value == ExceptionKnownType.OperationCanceled ||
            value == ExceptionKnownType.TaskCanceled;

        ///// <summary>
        ///// 是否为取消操作异常
        ///// </summary>
        ///// <param name="exception"></param>
        ///// <returns></returns>
        //[Obsolete("use GetKnownType", true)]
        //public static bool IsCanceledException(this Exception e) => GetKnownType(e).IsCanceledException();

        /// <summary>
        /// 通过 <see cref="Exception"/> 纪录日志并在 UI 上显示，传入 <see cref="LogLevel.None"/> 可不写日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="show"></param>
        /// <param name="logger"></param>
        /// <param name="tag"></param>
        /// <param name="level"></param>
        /// <param name="memberName"></param>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void LogAndShow(Exception? e,
            Action<string>? show,
            string tag, LogLevel level = LogLevel.Error,
            string memberName = "",
            string? msg = null, params object?[] args) => LogAndShowCore(e, show, tag, logger: null, level, memberName, msg, args);

        /// <inheritdoc cref="LogAndShow(Exception, Action{string}?, string, LogLevel, string?,string, string[])"/>
        public static void LogAndShow(Exception? e,
            Action<string>? show,
            ILogger? logger, LogLevel level = LogLevel.Error,
            string memberName = "",
            string? msg = null, params object?[] args) => LogAndShowCore(e, show, tag: null, logger, level, memberName, msg, args);

        static void LogAndShowCore(Exception? e,
            Action<string>? show,
            string? tag = null, ILogger? logger = null, LogLevel level = LogLevel.Error,
            string memberName = "",
            string? msg = null, params object?[] args)
        {
            bool has_msg = !string.IsNullOrWhiteSpace(msg);
            if (!has_msg)
            {
                if (!string.IsNullOrWhiteSpace(memberName))
                {
                    msg = $"{memberName} Error";
                    has_msg = true;
                }
            }
            var has_args = args.Any_Nullable();
            var has_e = e != null;
            if (has_e)
            {
                var knownType = e!.GetKnownType();
                if (knownType != ExceptionKnownType.Unknown) level = LogLevel.None;
            }
            var has_log_level = level < LogLevel.None;
            if (has_log_level)
            {
                if (logger == null && tag != null)
                {
                    logger = Log.CreateLogger(tag);
                }
                if (logger != null)
                {
                    if (has_args)
                    {
                        logger.Log(level, e, msg!, args);
                    }
                    else
                    {
                        logger.Log(level, e, msg!);
                    }
                }
            }
            show?.Invoke(GetShowMsg());
            string GetShowMsg()
            {
                if (has_e) return GetAllMessageCore(e!, has_msg, has_args, msg, args);
                if (has_msg)
                {
                    if (has_args)
                    {
                        return msg!.Format(args);
                    }
                    else
                    {
                        return msg!;
                    }
                }
                return "";
            }
        }

        /// <summary>
        /// 获取异常中所有错误信息
        /// </summary>
        /// <param name="e">当前捕获的异常</param>
        /// <param name="has_msg"></param>
        /// <param name="has_args"></param>
        /// <param name="msg">可选的消息，将写在第一行</param>
        /// <param name="args">可选的消息参数</param>
        /// <returns></returns>
        public static string GetAllMessage(this Exception e, string? msg = null, params object?[] args)
        {
            var has_msg = !string.IsNullOrWhiteSpace(msg);
            var has_args = args.Any_Nullable();
            return GetAllMessageCore(e, has_msg, has_args, msg, args);
        }

        /// <inheritdoc cref="GetAllMessage(Exception, string?, string[])"/>
        static string GetAllMessageCore(Exception e,
            bool has_msg, bool has_args,
            string? msg = null, params object?[] args)
        {
            StringBuilder sb = new();

            if (has_msg)
            {
                if (has_args)
                {
                    try
                    {
                        sb.AppendFormat(msg, args);
                    }
                    catch
                    {
                        sb.Append(msg);
                        foreach (var item in args)
                        {
                            sb.Append(' ');
                            sb.Append(item);
                        }
                    }
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(msg!);
                }
            }

            var exception = e;
            ushort i = 0;
            while (exception != null && i++ < byte.MaxValue)
            {
                var exception_message = exception.Message;
                if (!string.IsNullOrWhiteSpace(exception_message))
                {
                    sb.AppendLine(exception_message);
                }
                exception = exception.InnerException;
            }

            var text = sb.ToString().Trim();
            return text;
        }
    }

    /// <summary>
    /// 异常已知类型，通常不为 <see cref="Unknown"/> 的异常不需要纪录日志
    /// </summary>
    public enum ExceptionKnownType : byte
    {
        Unknown,

        /// <summary>
        /// 取消操作异常
        /// </summary>
        Canceled,

        /// <summary>
        /// 证书时间验证错误异常，通常为本地时间不正确导致 SSL 握手失败或服务端证书失效
        /// </summary>
        CertificateNotYetValid,

        /// <inheritdoc cref="OperationCanceledException"/>
        OperationCanceled,

        /// <inheritdoc cref="TaskCanceledException"/>
        TaskCanceled,
    }
}
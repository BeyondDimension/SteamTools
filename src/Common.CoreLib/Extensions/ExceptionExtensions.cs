using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ExceptionExtensions
    {
        static bool JavaInterop => OperatingSystem2.IsAndroid;

        public static ExceptionKnownType GetKnownType(this Exception e)
        {
            if (e is OperationCanceledException)
            {
                return ExceptionKnownType.Canceled;
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
        /// <param name="exception"></param>
        /// <returns></returns>
        [Obsolete("use GetKnownType")]
        public static bool IsCanceledException(this Exception e) => GetKnownType(e) == ExceptionKnownType.Canceled;

        /// <summary>
        /// 获取异常中所有错误信息
        /// </summary>
        /// <param name="e">当前捕获的异常</param>
        /// <param name="msg">可选的消息，将写在第一行</param>
        /// <param name="args">可选的消息参数</param>
        /// <returns></returns>
        public static string GetAllMessage(this Exception e, string? msg = null, params string[] args)
        {
            var has_msg = !string.IsNullOrWhiteSpace(msg);
            var has_args = args.Any_Nullable();
            return GetAllMessageCore(e, has_msg, has_args, msg, args);
        }

        /// <inheritdoc cref="GetAllMessage(Exception, string?, string[])" />
        public static string GetAllMessageCore(Exception e, bool has_msg, bool has_args, string? msg = null, params string[] args)
        {
            StringBuilder sb = new();
            if (has_msg)
            {
                if (has_args)
                {
                    sb.AppendFormat(msg, args);
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine(msg!);
                }
            }
            var e_str = e.ToString();
            sb.AppendLine(e_str);
            var stackTrace = e.StackTrace;
            if (!string.IsNullOrWhiteSpace(stackTrace) &&
                !e_str.Contains(stackTrace))
            {
                sb.AppendLine(stackTrace);
            }

            var exception = e;
            ushort i = 0;
            while (exception != null && i++ < byte.MaxValue)
            {
                var exception_message = exception.Message;
                if (!string.IsNullOrWhiteSpace(exception_message) &&
                    !e_str.Contains(exception_message))
                {
                    sb.AppendLine(exception_message);
                }
                exception = exception.InnerException;

            }

            var text = sb.ToString().Trim();
            return text;
        }
    }

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
    }
}
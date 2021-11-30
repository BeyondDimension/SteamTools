using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// 是否为取消操作异常
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static bool IsCanceledException(this Exception e)
        {
            if (e is TaskCanceledException || e is OperationCanceledException)
            {
                return true;
            }
            if (OperatingSystem2.IsAndroid)
            {
                if (e.Message == "Canceled" && e.GetType().FullName == "Java.IO.IOException")
                {
                    return true;
                }
            }
            return false;
        }

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
}
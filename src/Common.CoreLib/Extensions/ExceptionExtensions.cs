using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
            if (DI.Platform == Platform.Android)
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
        /// <param name="exception"></param>
        /// <param name="cycles">递归内部异常最大次数。</param>
        /// <returns></returns>
        static IEnumerable<string> GetMessages(this Exception exception, ushort cycles = byte.MaxValue)
        {
            ushort i = 0;
            while (exception != null && i++ < cycles)
            {
                var temp = exception.Message;
                exception = exception.InnerException;
                yield return temp;
            }
        }

        /// <summary>
        /// 获取异常中所有错误信息
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="separator">异常信息分隔符（默认换行）。</param>
        /// <param name="cycles">递归内部异常最大次数。</param>
        /// <returns></returns>
        public static string GetAllMessage(this Exception exception, string? separator = null, ushort cycles = byte.MaxValue)
            => string.Join(separator ?? Environment.NewLine, exception.GetMessages(cycles).Reverse());
    }
}
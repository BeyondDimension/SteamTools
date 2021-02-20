using System.Threading.Tasks;

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
    }
}
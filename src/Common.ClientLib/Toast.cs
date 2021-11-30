using Microsoft.Extensions.Logging;
using System.Application.Services;
using System.Text;

namespace System
{
    /// <inheritdoc cref="IToast"/>
    public static class Toast
    {
        /// <inheritdoc cref="IToast.Show(string, int?)"/>
        public static void Show(string text, int? duration = null)
        {
            var toast = DI.Get<IToast>();
            toast.Show(text, duration);
        }

        /// <inheritdoc cref="IToast.Show(string, ToastLength)"/>
        public static void Show(string text, ToastLength duration)
        {
            var toast = DI.Get<IToast>();
            toast.Show(text, duration);
        }

        /// <summary>
        /// 通过 <see cref="Exception"/> 显示 Toast 并纪录日志，传入 <see cref="LogLevel.None"/> 可不写日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="tag"></param>
        /// <param name="level"></param>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public static void Show(Exception e, string? tag = null, LogLevel level = LogLevel.Error, string? msg = null, params string[] args)
        {
            if (string.IsNullOrWhiteSpace(tag)) tag = nameof(Toast);
            var has_msg = !string.IsNullOrWhiteSpace(msg);
            var has_args = args.Any_Nullable();
            if (level < LogLevel.None)
            {
                var logger = Log.CreateLogger(tag);
                if (has_msg)
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
                else
                {
                    logger.Log(level, e, null);
                }
            }

            var text = ExceptionExtensions.GetAllMessageCore(e, has_msg, has_args, msg, args);
            Show(text, ToastLength.Long);
        }
    }
}
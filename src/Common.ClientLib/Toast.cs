using Microsoft.Extensions.Logging;
using System.Application.Services;
using System.Runtime.CompilerServices;

namespace System;

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

    static void ShowLong(string text) => Show(text, ToastLength.Long);

#if DEBUG
    [Obsolete("use e.LogAndShowT(..", true)]
    public static void Show(Exception e,
        string? tag = null, LogLevel level = LogLevel.Error,
        string? msg = null, params object?[] args) => e.LogAndShowT(tag, level, "", msg, args);
#endif

    /// <summary>
    /// 通过 <see cref="Exception"/> 纪录日志并在 Toast 上显示，传入 <see cref="LogLevel.None"/> 可不写日志
    /// </summary>
    /// <param name="e"></param>
    /// <param name="tag"></param>
    /// <param name="level"></param>
    /// <param name="memberName"></param>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public static void LogAndShowT(this Exception? e,
        string? tag = null, LogLevel level = LogLevel.Error,
        [CallerMemberName] string memberName = "",
        string? msg = null, params object?[] args)
        => ExceptionExtensions.LogAndShow(e,
            ShowLong,
            string.IsNullOrWhiteSpace(tag) ? nameof(Toast) : tag, level,
            memberName,
            msg, args);

    /// <summary>
    /// 通过 <see cref="Exception"/> 纪录日志并在 Toast 上显示，传入 <see cref="LogLevel.None"/> 可不写日志
    /// </summary>
    /// <param name="e"></param>
    /// <param name="logger"></param>
    /// <param name="level"></param>
    /// <param name="memberName"></param>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    public static void LogAndShowT(this Exception? e,
        ILogger logger, LogLevel level = LogLevel.Error,
        [CallerMemberName] string memberName = "",
        string? msg = null, params object?[] args) => ExceptionExtensions.LogAndShow(e,
            ShowLong,
            logger, level,
            memberName, msg, args);
}
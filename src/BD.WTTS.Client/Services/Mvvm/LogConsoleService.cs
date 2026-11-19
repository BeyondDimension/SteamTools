#if !ANDROID && !IOS
using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed partial class LogConsoleService : ReactiveObject
{
    static readonly Lazy<LogConsoleService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static LogConsoleService Current => mCurrent.Value;

    LogConsoleService()
    {

    }

    #region UI 进程部分

    readonly Source accelerator = new();
    readonly Source backend = new();

    /// <summary>
    /// 加速进程的日志
    /// </summary>
    public string? LogMessageAccelerator => accelerator.ToString();

    /// <summary>
    /// 后端进程的日志
    /// </summary>
    public string? LogMessageBackEnd => backend.ToString();

    partial class Source
    {
        int notificationPending;

        /// <summary>
        /// 追加收到的日志 UTF-8 字节并触发属性通知
        /// </summary>
        public void AppendWithPropertyChanged(IReactiveObject o, byte[] value, string propertyName)
        {
            Append(value);
            if (Interlocked.Exchange(ref notificationPending, 1) != 0)
                return;

            Dispatcher.UIThread.Post(() =>
            {
                Interlocked.Exchange(ref notificationPending, 0);
                o.RaisePropertyChanged(propertyName);
            });
        }
    }

    /// <summary>
    /// 追加收到的日志 UTF-8 字节
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="value"></param>
    internal void WriteMessage(string? moduleName, byte[]? value)
    {
        if (value is not { Length: > 0 })
            return;

        switch (moduleName)
        {
            case AssemblyInfo.Accelerator:
                accelerator.AppendWithPropertyChanged(this, value, nameof(LogMessageAccelerator));
                break;
            case IPlatformService.IPCRoot.moduleName:
                backend.AppendWithPropertyChanged(this, value, nameof(LogMessageBackEnd));
                break;
        }
    }

    #endregion
}
#endif

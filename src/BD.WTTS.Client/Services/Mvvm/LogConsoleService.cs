#if !ANDROID && !IOS
using Avalonia.Threading;
using Utf8StringInterpolation;

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

    readonly string?[] logMessages = new string?[2];

    /// <summary>
    /// 加速进程的日志
    /// </summary>
    public string? LogMessageAccelerator => logMessages[0];

    /// <summary>
    /// 后端进程的日志
    /// </summary>
    public string? LogMessageBackEnd => logMessages[1];

    /// <summary>
    /// 日志源
    /// </summary>
    readonly Dictionary<string, Source> sources = new();

    partial class Source
    {
        /// <summary>
        /// 追加收到的日志 UTF-8 字节并触发属性通知
        /// </summary>
        public void AppendWithPropertyChanged(IReactiveObject o, byte[]? value,
            string propertyName, ref string? propertyValue)
        {
            if (value != null && value.Length > 0)
            {
                Append(value);

                var logAllMessage = ToString();
                propertyValue = logAllMessage;

                Dispatcher.UIThread.Invoke(() =>
                {
                    o.RaisePropertyChanged(propertyName);
                });

            }
        }
    }

    /// <summary>
    /// 追加收到的日志 UTF-8 字节
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="value"></param>
    internal void WriteMessage(string? moduleName, byte[]? value)
    {
        if (!string.IsNullOrWhiteSpace(moduleName) &&
            value != null && value.Length > 0)
        {
            if (!sources.TryGetValue(moduleName, out var source))
            {
                source = new(
#if DEBUG
                    moduleName
#endif
                    );
                sources.Add(moduleName, source);
            }

            switch (moduleName)
            {
                case AssemblyInfo.Accelerator:
                    source.AppendWithPropertyChanged(this, value,
                        nameof(LogMessageAccelerator), ref logMessages[0]);
                    break;
                case IPlatformService.IPCRoot.moduleName:
                    source.AppendWithPropertyChanged(this, value,
                        nameof(LogMessageBackEnd), ref logMessages[1]);
                    break;
            }
        }
    }

    #endregion
}
#endif
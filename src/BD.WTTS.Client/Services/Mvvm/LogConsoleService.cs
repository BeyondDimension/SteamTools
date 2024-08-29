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

    /// <summary>
    /// 日志源
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    sealed class Source
    {
#if DEBUG
        readonly string moduleName;

        public Source(string moduleName)
        {
            this.moduleName = moduleName;
        }

        string DebuggerDisplay() =>
$"""
moduleName: {moduleName}
{ToString()}
""";
#endif

        (string? str, long byteLength)? message = null;
        Utf8StringBuilder? builder = null;

        /// <summary>
        /// 获取所有收到的日志字符串
        /// </summary>
        /// <returns></returns>
        string? GetMessage()
        {
            if (builder == null)
            {
                message = null;
                return null;
            }
            else
            {
                var result = builder.ToString();
                message = new(result, builder.ByteLength);
                return result;
            }
        }

        /// <summary>
        /// 追加收到的日志 UTF-8 字节并触发属性通知
        /// </summary>
        public void AppendWithPropertyChanged(IReactiveObject o, byte[]? value,
            string propertyName, ref string? propertyValue)
        {
            if (value != null && value.Length > 0)
            {
                // 追加收到的日志 UTF-8 字节
                (builder ??= new()).Append(value);

                var logAllMessage = ToString();
                propertyValue = logAllMessage;

                Dispatcher.UIThread.Invoke(() =>
                {
                    o.RaisePropertyChanged(propertyName);
                });

            }
        }

        public override string? ToString()
        {
            if (message != null)
            {
                if (builder == null)
                {
                    return message.Value.str;
                }
                else
                {
                    if (builder.ByteLength == message.Value.byteLength)
                    {
                        return message.Value.str;
                    }
                    else
                    {
                        return GetMessage();
                    }
                }
            }
            else
            {
                if (builder != null)
                {
                    return GetMessage();
                }
            }

            return null;
        }

        /// <summary>
        /// U8 字符串版的 <see cref="StringBuilder"/>
        /// </summary>
        sealed class Utf8StringBuilder
        {
            List<byte[]>? chunk;

            public long ByteLength { get; private set; }

            public void Append(byte[] value)
            {
                (chunk ??= new()).Add(value);
                ByteLength += value.Length;
            }

            public override string? ToString()
            {
                if (chunk == null)
                {
                    return null;
                }
                else if (chunk.Count <= 0)
                {
                    return string.Empty;
                }

                using var buffer = Utf8String.CreateWriter(out var writer);

                foreach (var it in chunk)
                {
                    writer.AppendUtf8(it);
                }

                writer.Flush();

                var result = buffer.ToString();
                return result;
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
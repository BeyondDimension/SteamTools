#if MACOS || MACCATALYST
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Logging;

/// <inheritdoc cref="ClientLogger"/>
public sealed partial class NSLogger : ClientLogger
{
    public NSLogger(string name) : base(name)
    {
    }

    public override bool IsEnabled(LogLevel logLevel)
    {
#if DEBUG
        return true;
#else
        return logLevel >= LogLevel.Error;
#endif
    }

    public override void WriteMessage(LogLevel logLevel, string message)
    {
        NSLog(message);
    }

    [LibraryImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
    static partial void NSLog(IntPtr format, [MarshalAs(UnmanagedType.LPWStr)] string s);

    static void NSLog(string format, params object[]? args)
    {
#pragma warning disable CS0618 // 类型或成员已过时
        var fmt = NSString.CreateNative("%s");
#pragma warning restore CS0618 // 类型或成员已过时
        var val = (args == null || args.Length == 0) ? format : string.Format(format, args);

        NSLog(fmt, val);
        NSString.ReleaseNative(fmt);
    }
}
#endif

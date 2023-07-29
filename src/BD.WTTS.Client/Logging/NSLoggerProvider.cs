#if MACOS || MACCATALYST
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Logging;

[ProviderAlias("NSLog")]
public sealed class NSLoggerProvider : ILoggerProvider
{
    private NSLoggerProvider() { }

    public ILogger CreateLogger(string name)
    {
        return new NSLogger(name);
    }

    void IDisposable.Dispose()
    {
    }

    public static ILoggerProvider Instance { get; } = new NSLoggerProvider();
}
#endif
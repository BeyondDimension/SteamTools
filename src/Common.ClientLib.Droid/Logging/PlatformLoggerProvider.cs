using Microsoft.Extensions.Logging;

namespace System.Logging
{
    [ProviderAlias("Droid")]
    public class PlatformLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string name)
        {
            return new PlatformLogger(name);
        }

        void IDisposable.Dispose()
        {
        }

        public static ILoggerProvider Instance { get; } = new PlatformLoggerProvider();
    }
}
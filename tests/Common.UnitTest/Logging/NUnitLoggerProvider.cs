using Microsoft.Extensions.Logging;

namespace System.Logging
{
    [ProviderAlias("NUnit")]
    public class NUnitLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string name)
        {
            return new NUnitLogger(name);
        }

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable CA1816 // Dispose 方法应调用 SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose 方法应调用 SuppressFinalize
#pragma warning restore IDE0079 // 请删除不必要的忽略
        {
        }

        static readonly Lazy<ILoggerProvider> mInstance
          = new Lazy<ILoggerProvider>(() => new NUnitLoggerProvider());

        public static ILoggerProvider Instance => mInstance.Value;
    }
}
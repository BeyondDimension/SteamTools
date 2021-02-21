namespace System.Logging
{
    /// <summary>
    /// An empty scope without any logic
    /// <para>https://github.com/dotnet/extensions/blob/v3.1.5/src/Logging/shared/NullScope.cs</para>
    /// <para>https://github.com/dotnet/runtime/blob/v5.0.0-rtm.20519.4/src/libraries/Common/src/Extensions/Logging/NullScope.cs</para>
    /// </summary>
    public class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        private NullScope()
        {
        }

#pragma warning disable CA1816 // Dispose 方法应调用 SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose 方法应调用 SuppressFinalize
        {
        }
    }
}
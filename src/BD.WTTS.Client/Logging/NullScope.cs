// ReSharper disable once CheckNamespace
namespace BD.WTTS.Logging;

/// <summary>
/// An empty scope without any logic
/// <para>https://github.com/dotnet/extensions/blob/v3.1.5/src/Logging/shared/NullScope.cs</para>
/// <para>https://github.com/dotnet/runtime/blob/v5.0.0-rtm.20519.4/src/libraries/Common/src/Extensions/Logging/NullScope.cs</para>
/// </summary>
sealed class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new();

    NullScope()
    {
    }

    public void Dispose()
    {
    }
}

// ReSharper disable once CheckNamespace
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;

namespace Avalonia.Controls;

public static class IPlatformLifetimeEventsImplExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetShutdownRequested(this IPlatformLifetimeEventsImpl platformLifetime, EventHandler<ShutdownRequestedEventArgs>? handler)
    {
        platformLifetime.ShutdownRequested += handler;
    }
}

using Avalonia.Controls;

namespace Avalonia;

public static class ClassicDesktopStyleApplicationLifetimeExtensions
{
    // Keep the existing entry point while using Avalonia's current activation and shutdown handling.
    public static int StartWithClassicDesktopLifetime2(
        this AppBuilder builder, string[] args, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
    {
        return builder.StartWithClassicDesktopLifetime(args, shutdownMode);
    }
}
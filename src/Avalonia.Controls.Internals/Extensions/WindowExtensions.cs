// ReSharper disable once CheckNamespace
namespace Avalonia.Controls;

public static class AvaloniaControlsInternalsWindowExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CloseCore_(this Window window, WindowCloseReason reason, bool isProgrammatic) => window.CloseCore(reason, isProgrammatic);
}

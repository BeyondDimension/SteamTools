// ReSharper disable once CheckNamespace
namespace Avalonia.Controls;

public static class AvaloniaControlsInternalsWindowExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CloseCore_(this Window window, WindowCloseReason reason, bool isProgrammatic, bool ignoreCancel) => window.CloseCore(reason, isProgrammatic, ignoreCancel);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HandleResized_(this Window window, Size clientSize, WindowResizeReason reason) => window.HandleResized(clientSize, reason);
}

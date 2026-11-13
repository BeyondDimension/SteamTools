#if LINUX
using Avalonia.Input.Platform;
using static System.Net.Mime.MediaTypeNames;

namespace BD.WTTS.Services.Implementation;

public sealed class AvaloniaClipboardPlatformService : IClipboardPlatformService
{
    public bool PlatformHasText => !string.IsNullOrEmpty(PlatformGetTextAsync().GetAwaiter().GetResult());

    public async Task<string> PlatformGetTextAsync()
    {
        TopLevel? topLevel = App.Instance.MainWindow;
        var clipboard = topLevel != null
            ? topLevel.Clipboard
            : AvaloniaLocator.Current.GetService<IClipboard>();
        if (clipboard != null)
        {
            var result = await clipboard.TryGetTextAsync();
            return result ?? string.Empty;
        }
        return string.Empty;
    }

    public async Task PlatformSetTextAsync(string text)
    {
        TopLevel? topLevel = App.Instance.MainWindow;
        var clipboard = topLevel != null
            ? topLevel.Clipboard
            : AvaloniaLocator.Current.GetService<IClipboard>();
        if (clipboard != null)
        {
            //不能用 await 等待 Linux 上不知啥原因导致卡死
            clipboard.SetTextAsync(text).GetAwaiter();
            await Task.CompletedTask;
        }
    }
}
#endif

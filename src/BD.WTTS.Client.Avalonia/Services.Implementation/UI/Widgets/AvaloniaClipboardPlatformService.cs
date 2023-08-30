#if LINUX
using static System.Net.Mime.MediaTypeNames;

namespace BD.WTTS.Services.Implementation;

public sealed class AvaloniaClipboardPlatformService : IClipboardPlatformService
{
    public bool PlatformHasText => !string.IsNullOrEmpty(PlatformGetTextAsync().GetAwaiter().GetResult());

    public async Task<string> PlatformGetTextAsync()
    {
        TopLevel? topLevel = App.Instance.MainWindow;
        if (topLevel != null)
        {
            var clipboard = topLevel.Clipboard;
            if (clipboard != null)
            {
                var result = await clipboard.GetTextAsync();
                return result ?? string.Empty;
            }
        }
        return string.Empty;
    }

    public async Task PlatformSetTextAsync(string text)
    {
        TopLevel? topLevel = App.Instance.MainWindow;
        if (topLevel != null)
        {
            var clipboard = topLevel.Clipboard;
            if (clipboard != null)
            {
                //不能用 await 等待 Linux 上不知啥原因导致卡死
                clipboard.SetTextAsync(text).GetAwaiter();
                await Task.CompletedTask;
            }
        }
    }
}
#endif
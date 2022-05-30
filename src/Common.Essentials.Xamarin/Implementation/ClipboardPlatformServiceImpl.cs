namespace System.Application.Services.Implementation;

sealed class ClipboardPlatformServiceImpl : IClipboardPlatformService
{
    bool IClipboardPlatformService.PlatformHasText
        => Clipboard.HasText;

    async Task<string> IClipboardPlatformService.PlatformGetTextAsync()
         => await Clipboard.GetTextAsync() ?? string.Empty;

    Task IClipboardPlatformService.PlatformSetTextAsync(string text)
        => Clipboard.SetTextAsync(text);
}

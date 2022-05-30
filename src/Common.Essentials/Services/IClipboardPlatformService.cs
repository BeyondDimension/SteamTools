namespace System.Application.Services;

public interface IClipboardPlatformService
{
    static IClipboardPlatformService Instance => DI.Get<IClipboardPlatformService>();

    Task PlatformSetTextAsync(string text);

    Task<string> PlatformGetTextAsync();

    bool PlatformHasText { get; }
}
using System.Application.Services;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Application;

/// <summary>
/// 剪贴板，参考 Essentials.Clipboard。
/// <para><see cref="https://docs.microsoft.com/zh-cn/xamarin/essentials/clipboard"/></para>
/// <para><see cref="https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/Clipboard/Clipboard.shared.cs"/></para>
/// </summary>
public static class Clipboard2
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task SetTextAsync(string? text) => await IClipboardPlatformService.Instance.PlatformSetTextAsync(text ?? string.Empty);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async void SetText(string? text) => await SetTextAsync(text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<string> GetTextAsync() => await IClipboardPlatformService.Instance.PlatformGetTextAsync();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasText() => IClipboardPlatformService.Instance.PlatformHasText;
}
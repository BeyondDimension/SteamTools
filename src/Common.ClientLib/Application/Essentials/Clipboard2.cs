using System.Threading.Tasks;
using Xamarin.Essentials;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    /// <summary>
    /// 剪贴板，参考 Xamarin.Essentials.Clipboard
    /// <para><see cref="https://docs.microsoft.com/zh-cn/xamarin/essentials/clipboard"/></para>
    /// <para><see cref="https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/Clipboard/Clipboard.shared.cs"/></para>
    /// </summary>
    public static class Clipboard2
    {
        public static async Task SetTextAsync(string? text)
        {
            if (Essentials.IsSupported)
            {
                await Clipboard.SetTextAsync(text);
            }
            else
            {
                await IClipboardPlatformService.Instance.PlatformSetTextAsync(text ?? string.Empty);
            }
        }

        public static async void SetText(string? text) => await SetTextAsync(text);

        public static async Task<string> GetTextAsync()
        {
            if (Essentials.IsSupported)
            {
                return await Clipboard.GetTextAsync();
            }
            else
            {
                return await IClipboardPlatformService.Instance.PlatformGetTextAsync();
            }
        }

        public static bool HasText
        {
            get
            {
                if (Essentials.IsSupported)
                {
                    return Clipboard.HasText;
                }
                else
                {
                    return IClipboardPlatformService.Instance.PlatformHasText;
                }
            }
        }
    }
}
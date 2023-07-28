using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    /// <summary>
    /// 通用复制字符串到剪贴板，并在成功后显示 toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="msgToast"></param>
    /// <param name="showToast"></param>
    /// <returns></returns>
    static async Task CopyToClipboardAsync(string? text, string? msgToast = null, bool showToast = true)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            await Clipboard2.SetTextAsync(text);
            if (showToast) Toast.Show(ToastIcon.Success, msgToast ?? AppResources.CopyToClipboard);
        }
    }
}
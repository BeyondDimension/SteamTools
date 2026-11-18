using static BD.WTTS.Services.IPCToastService;
using AppResources = BD.WTTS.Client.Resources.Strings;
using BDToastIcon = BD.Common.Enums.ToastIcon;
using ToastIcon = BD.WTTS.Services.IPCToastService.ToastIcon;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class IPCMainProcessServiceImpl : IPCToastService
{
    static string GetText(ToastText text) => text switch
    {
        ToastText.CreateCertificateFaild => AppResources.CreateCertificateFaild,
        ToastText.CommunityFix_DNSErrorNotify => AppResources.CommunityFix_DNSErrorNotify,
        ToastText.CommunityFix_DnsPollutionResponse => BD.WTTS.Client.Resources.ProxyStrings.DnsPollution(AppResources.Culture),
        ToastText.CommunityFix_OnRunCatch => AppResources.CommunityFix_OnRunCatch,
        _ => string.Empty,
    };

    public Task<string?> GetTextAsync(ToastText text) => Task.FromResult<string?>(GetText(text));

    public void Show(ToastIcon icon, ToastText text, int? duration = null)
    {
        var icon_ = (BDToastIcon)icon;
        var text_ = GetText(text);
        Toast.Show(icon_, text_, duration);
    }

    public void Show(ToastIcon icon, ToastText text, ToastLength duration)
    {
        var icon_ = (BDToastIcon)icon;
        var text_ = GetText(text);
        Toast.Show(icon_, text_, duration);
    }

    public void Show(ToastIcon icon, ToastText text, int? duration = null, params string?[] args)
    {
        var icon_ = (BDToastIcon)icon;
        var text_ = GetText(text).Format(args);
        Toast.Show(icon_, text_, duration);
    }

    public void Show(ToastIcon icon, ToastText text, ToastLength duration, params string?[] args)
    {
        var icon_ = (BDToastIcon)icon;
        var text_ = GetText(text).Format(args);
        Toast.Show(icon_, text_, duration);
    }

    public void Show(ToastIcon icon, ToastText text, params string?[] args)
    {
        var icon_ = (BDToastIcon)icon;
        var text_ = GetText(text).Format(args);
        Toast.Show(icon_, text_);
    }

    public void ShowAppend(ToastIcon icon, ToastText text, int? duration = null, string? appendText = null)
    {
        var icon_ = (BDToastIcon)icon;
        var text_ = GetText(text) + appendText;
        Toast.Show(icon_, text_, duration);
    }

    public void ShowAppend(ToastIcon icon, ToastText text, ToastLength duration, string? appendText)
    {
        var icon_ = (BDToastIcon)icon;
        var text_ = GetText(text) + appendText;
        Toast.Show(icon_, text_, duration);
    }

    public void ShowAppend(ToastIcon icon, ToastText text, string? appendText)
    {
        var icon_ = (BDToastIcon)icon;
        var text_ = GetText(text) + appendText;
        Toast.Show(icon_, text_);
    }
}
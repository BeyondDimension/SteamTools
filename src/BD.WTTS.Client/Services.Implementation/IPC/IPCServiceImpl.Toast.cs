using static BD.WTTS.Services.IPCToastService;
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class IPCServiceImpl : IPCToastService
{
    static string GetText(ToastText text) => text switch
    {
        ToastText.CreateCertificateFaild => AppResources.CreateCertificateFaild,
        ToastText.CommunityFix_DNSErrorNotify => AppResources.CommunityFix_DNSErrorNotify,
        ToastText.CommunityFix_OnRunCatch => AppResources.CommunityFix_OnRunCatch,
        _ => string.Empty,
    };

    public void Show(ToastText text, int? duration = null)
    {
        Toast.Show(GetText(text), duration);
    }

    public void Show(ToastText text, ToastLength duration)
    {
        Toast.Show(GetText(text), duration);
    }

    public void Show(ToastText text, int? duration = null, params object?[] args)
    {
        Toast.Show(GetText(text).Format(args), duration);
    }

    public void Show(ToastText text, ToastLength duration, params object?[] args)
    {
        Toast.Show(GetText(text).Format(args), duration);
    }

    public void Show(ToastText text, params object?[] args)
    {
        Toast.Show(GetText(text).Format(args));
    }

    public void ShowAppend(ToastText text, int? duration = null, string? appendText = null)
    {
        Toast.Show(GetText(text) + appendText, duration);
    }

    public void ShowAppend(ToastText text, ToastLength duration, string? appendText)
    {
        Toast.Show(GetText(text) + appendText, duration);
    }

    public void ShowAppend(ToastText text, string? appendText)
    {
        Toast.Show(GetText(text) + appendText);
    }
}
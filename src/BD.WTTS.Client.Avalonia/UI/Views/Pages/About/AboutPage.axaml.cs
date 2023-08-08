using Avalonia.Controls;
using BD.WTTS.Services;

namespace BD.WTTS.UI.Views.Pages;

public partial class AboutPage : ReactiveUserControl<AboutPageViewModel>
{
    public AboutPage()
    {
        InitializeComponent();
        DataContext = IViewModelManager.Instance.Get<AboutPageViewModel>();
    }

    /// <summary>
    /// 点击间隔时间（秒）设置一个间隔时间避免频繁点击
    /// </summary>
    const double clickOpenBrowserIntervalSeconds = .75d;
    readonly Dictionary<string, DateTime> clickOpenBrowserTimeRecord = new();

    void OpenBrowser(string url)
    {
        try
        {
            var hasKey = clickOpenBrowserTimeRecord.TryGetValue(url, out var dt);
            var now = DateTime.Now;
            if (hasKey && (now - dt).TotalSeconds <= clickOpenBrowserIntervalSeconds)
                return;
            Browser2.Open(url);
            if (!clickOpenBrowserTimeRecord.TryAdd(url, now)) clickOpenBrowserTimeRecord[url] = now;
        }
        catch (Exception ex)
        {
            Toast.LogAndShowT(ex);
        }

    }

    void GPLv3_Tapped(object? sender, TappedEventArgs e)
    {
        OpenBrowser("https://www.gnu.org/licenses/gpl-3.0.html");
    }

    void Dotnet_Tapped(object? sender, TappedEventArgs e)
    {
        OpenBrowser("https://dotnet.microsoft.com");
    }

    void Avalonia_Tapped(object? sender, TappedEventArgs e)
    {
        OpenBrowser("https://avaloniaui.net");
    }
}

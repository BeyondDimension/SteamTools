using Avalonia.Controls;
using BD.SteamClient.Constants;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class IdleCardPage : PageBase<IdleCardPageViewModel>
{
    public IdleCardPage()
    {
        InitializeComponent();
        DataContext ??= new IdleCardPageViewModel();
    }

    private void AppOpenLink_MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is IdleApp app)
        {
            var url = item.Tag switch
            {
                "steamdb" => string.Format(SteamApiUrls.STEAMDBINFO_APP_URL, app.AppId),
                "steamcardexchange" => string.Format(SteamApiUrls.STEAMCARDEXCHANGE_APP_URL, app.AppId),
                _ => string.Format(SteamApiUrls.STEAMSTORE_APP_URL, app.AppId),
            };
            ViewModel?.OpenLinkUrlCommand.Execute(url);
        }
    }
}

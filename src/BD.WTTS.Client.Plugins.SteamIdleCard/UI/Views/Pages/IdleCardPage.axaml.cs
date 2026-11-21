using Avalonia.Controls;
using Avalonia.Data.Converters;
using BD.SteamClient.Constants;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class IdleCardPage : PageBase<IdleCardPageViewModel>
{
    public static IValueConverter RunOrStopIconConverter { get; } = new FuncValueConverter<bool, Symbol>(
        static running => running ? Symbol.StopFilled : Symbol.PlayFilled);

    public static IValueConverter RunOrStopTextConverter { get; } = new FuncValueConverter<bool, string>(
        static running => running ? Strings.Stop : Strings.Launch);

    public static IValueConverter BlacklistActionTextConverter { get; } = new FuncValueConverter<bool, string>(
        static blacklisted => blacklisted ? Strings.L10nAuthIdle_Text065 : Strings.L10nAuthIdle_Text064);

    public IdleCardPage()
    {
        InitializeComponent();
        DataContext ??= IViewModelManager.Instance.Get<IdleCardPageViewModel>();
    }

    private void AppOpenLink_MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is FAMenuFlyoutItem { DataContext: IdleApp app } item)
        {
            var url = item.Tag switch
            {
                "steamdb" => string.Format(SteamApiUrls.STEAMDBINFO_APP_URL, app.AppId),
                "steamcardexchange" => string.Format(SteamApiUrls.STEAMCARDEXCHANGE_APP_URL, app.AppId),
                _ => string.Format(SteamApiUrls.STEAMSTORE_APP_URL, app.AppId),
            };
            var command = ViewModel?.OpenLinkUrlCommand;
            if (command?.CanExecute(url) == true)
                command.Execute(url);
        }
    }
}

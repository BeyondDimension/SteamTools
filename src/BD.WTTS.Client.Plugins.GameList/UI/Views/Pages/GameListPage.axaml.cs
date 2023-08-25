using Avalonia.Layout;
using BD.SteamClient.Constants;
using BD.WTTS.Client.Resources;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class GameListPage : PageBase<GameListPageViewModel>
{
    public GameListPage()
    {
        InitializeComponent();
        DataContext ??= new GameListPageViewModel();

        //GameLibrarySettings.GameLibraryLayoutType.Subscribe(x =>
        //{
        //    GameListRepeater.Layout = GetGameLayout(x);
        //});
    }

    //static VirtualizingLayout GetGameLayout(GridLayoutType layoutType)
    //{
    //    VirtualizingLayout layout = layoutType switch
    //    {
    //        GridLayoutType.List => new StackLayout()
    //        {
    //            Orientation = Orientation.Vertical,
    //            Spacing = 10,
    //        },
    //        GridLayoutType.CompactGrid => new UniformGridLayout()
    //        {
    //            MinItemWidth = 230,
    //            MinItemHeight = 65,
    //            MinColumnSpacing = 5,
    //            MinRowSpacing = 5,
    //            ItemsJustification = UniformGridLayoutItemsJustification.Start,
    //            ItemsStretch = UniformGridLayoutItemsStretch.Uniform,
    //        },
    //        _ => new UniformGridLayout()
    //        {
    //            MinItemWidth = 150,
    //            MinItemHeight = 310,
    //            MinColumnSpacing = 10,
    //            MinRowSpacing = 10,
    //            ItemsJustification = UniformGridLayoutItemsJustification.Start,
    //            ItemsStretch = UniformGridLayoutItemsStretch.Uniform,
    //        },
    //    };

    //    return layout;
    //}

    private void AppOpenLink_MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is SteamApp app)
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

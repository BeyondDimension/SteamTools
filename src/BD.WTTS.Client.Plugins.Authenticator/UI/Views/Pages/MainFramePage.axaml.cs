using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : UserControl
{
    public MainFramePage()
    {
        InitializeComponent();

        InnerNavFrame.NavigationFailed += (_, args) =>
        {
            Log.Error(nameof(MainFramePage), args.Exception,
                "Could not navigate to nested page {page}.", args.SourcePageType?.FullName);
            Toast.Show(ToastIcon.Error, Strings.Error_PageFault);
        };

        Tabs.SelectionChanged += Tabs_SelectionChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Tabs.SelectedIndex < 0 && Tabs.ItemCount > 0)
            Tabs.SelectedIndex = 0;
        else
            ShowSelectedPage();
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && e.Source == Tabs)
            ShowSelectedPage();
    }

    private void ShowSelectedPage()
    {
        var pageType = Tabs.SelectedItem switch
        {
            var item when ReferenceEquals(item, AuthenticatorTab) => typeof(AuthenticatorHomePage),
            var item when ReferenceEquals(item, SteamMarketTab) => typeof(SteamMarketManagePage),
            _ => null,
        };

        if (pageType is not null && InnerNavFrame.CurrentSourcePageType != pageType)
            InnerNavFrame.Navigate(pageType, null, FrameEffect.NoTransition);
    }
}

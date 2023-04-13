using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainView : ReactiveUserControl<MainWindowViewModel>
{
    private static IReadOnlyDictionary<Type, Type> PageTypes { get; }

    static MainView()
    {
        PageTypes = new Dictionary<Type, Type>
            {
                //{ typeof(StartPageViewModel), typeof(StartPage) },
                //{ typeof(CommunityProxyPageViewModel), typeof(CommunityProxyPage) },
                //{ typeof(ProxyScriptManagePageViewModel), typeof(ProxyScriptManagePage) },
                //{ typeof(SteamAccountPageViewModel), typeof(SteamAccountPage) },
                //{ typeof(AboutPageViewModel), typeof(AboutPage) },
                //{ typeof(GameListPageViewModel), typeof(GameListPage) },
                //{ typeof(LocalAuthPageViewModel), typeof(LocalAuthPage) },
                //{ typeof(GameRelatedPageViewModel), typeof(GameRelatedPage) },
                //{ typeof(ArchiSteamFarmPlusPageViewModel), typeof(ArchiSteamFarmPlusPage) },
                ////{ typeof(GameRelated_BorderlessPageViewModel), typeof(GameRelated_BorderlessPage) },
                //{ typeof(AccountPageViewModel), typeof(AccountPage) },
                { typeof(SettingsPageViewModel), typeof(SettingsPage) },
#if DEBUG
                { typeof(DebugPageViewModel), typeof(DebugPage) },
//#if WINDOWS
//                { typeof(DebugWebViewPageViewModel), typeof(DebugWebViewPage) },
//#endif
#endif
            };
    }

    public MainView()
    {
        InitializeComponent();

        //DebugButton.Click += (s, e) =>
        //{
        //    var window = new DebugWindow();
        //    window.Show();
        //};

        BackViewButton.Click += (s, e) =>
        {
            FrameView.GoBack();
        };

        NavView.SelectionChanged += (s, e) =>
        {
            if (NavView.SelectedItem != null)
            {
                try
                {
                    FrameView?.Navigate(PageTypes[NavView.SelectedItem.GetType()]);
                }
                catch
                {
                    FrameView?.GoBack();
                }
            }
        };
    }
}

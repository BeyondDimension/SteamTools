namespace BD.WTTS.UI.Views.Pages;

public sealed partial class MainView : ReactiveUserControl<MainWindowViewModel>
{
    public Dictionary<Type, Type>? MenuTabItemToUserControls { get; private set; }

    public MainView()
    {
        InitializeComponent();

        NavView.SelectionChanged += (_, e) =>
        {
            if (e.SelectedItem != null && MenuTabItemToUserControls != null)
            {
                var vmType = e.SelectedItem.GetType();
                if (MenuTabItemToUserControls.TryGetValue(vmType, out var pageType))
                {
                    FrameView?.Navigate(pageType);
                    return;
                }
            }
            FrameView?.Navigate(typeof(ErrorPage));
        };
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (ViewModel != null)
        {
            IEnumerable<KeyValuePair<Type, Type>> menuTabItemToUserControls = new KeyValuePair<Type, Type>[]
            {
                new KeyValuePair<Type, Type>(typeof(HomeMenuTabItemViewModel), typeof(HomePage)),
                new KeyValuePair<Type, Type>(typeof(DebugMenuTabItemViewModel), typeof(DebugPage)),
                new KeyValuePair<Type, Type>(typeof(SettingsMenuTabItemViewModel), typeof(SettingsPage)),
                //new KeyValuePair<Type, Type>(typeof(AboutPageViewModel), typeof(AboutPage)),
            };

            if (Startup.Instance.TryGetPlugins(out var plugins))
            {
                menuTabItemToUserControls = menuTabItemToUserControls.Concat(plugins
                    .Select(static x =>
                    {
                        try
                        {
                            return x.GetMenuTabItemToPages();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(nameof(MainWindowViewModel), ex,
                                $"({x.Name}) Plugin.GetMenuTabItemToPages fail.");
                            return null;
                        }
                    })
                    .Where(static x => x != null)
                    .SelectMany(static x => x!));
            }

            MenuTabItemToUserControls = new(menuTabItemToUserControls);
        }
    }
}

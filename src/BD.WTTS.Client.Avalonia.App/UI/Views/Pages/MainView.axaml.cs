namespace BD.WTTS.UI.Views.Pages;

public sealed partial class MainView : ReactiveUserControl<MainWindowViewModel>
{
    public Dictionary<Type, Type>? MenuTabItemToUserControls { get; private set; }

    public MainView()
    {
        InitializeComponent();

        NavView.SelectionChanged += (_, _) =>
        {
            if (NavView.SelectedItem != null)
            {
                if (MenuTabItemToUserControls != null)
                {
                    var vmType = NavView.SelectedItem.GetType();
                    if (MenuTabItemToUserControls.TryGetValue(vmType, out var pageType))
                    {
                        FrameView?.Navigate(pageType);
                        return;
                    }
                }
                FrameView?.GoBack();
            }
        };
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (ViewModel != null)
        {
            IEnumerable<KeyValuePair<Type, Type>> menuTabItemToUserControls = new KeyValuePair<Type, Type>[]
            {
                //new KeyValuePair<Type, Type>(typeof(WelcomePageViewModel), typeof()),
                new KeyValuePair<Type, Type>(typeof(DebugPageViewModel), typeof(DebugPage)),
                new KeyValuePair<Type, Type>(typeof(SettingsPageViewModel), typeof(SettingsPage)),
                //new KeyValuePair<Type, Type>(typeof(AboutPageViewModel), typeof(AboutPage)),
            };

            if (Startup.Instance.TryGetPlugins(out var plugins))
            {
                menuTabItemToUserControls = menuTabItemToUserControls.Concat(plugins
                    .Select(static x =>
                    {
                        try
                        {
                            return x.GetPageToUserControls();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(nameof(MainWindowViewModel), ex,
                                $"({x.Name}) Plugin.GetPageToUserControls fail.");
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

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : UserControl
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ArchiSteamFarmPage))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(BotPage))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ASFSettingsPage))]
    public MainFramePage()
    {
        InitializeComponent();
        InnerNavFrame.NavigationFailed += (_, args) => Log.Error(nameof(MainFramePage), args.Exception,
            "Could not navigate to nested page {page}.", args.SourcePageType?.FullName);

        Tabs.SelectionChanged += Tabs_SelectionChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Tabs.SelectedIndex < 0 && Tabs.ItemCount > 0)
            Tabs.SelectedIndex = 0;

        Tabs_SelectionChanged(null, null);
    }

    private void Tabs_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs? e)
    {
        if (Tabs.SelectedItem is TabStripItem tab && tab.Tag is Type t)
        {
            if (InnerNavFrame.CurrentSourcePageType == t)
                return;

            InnerNavFrame.Navigate(t, null, FrameEffect.NoTransition);
        }
    }
}

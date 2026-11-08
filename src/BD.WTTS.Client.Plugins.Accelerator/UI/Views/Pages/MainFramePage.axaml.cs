using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Media.Animation;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : UserControl
{
    int lastIndex = -1;
    int waitingGeneration;

    public MainFramePage()
    {
        InitializeComponent();

        //Tabs.Items.Add(new TabStripItem { Content = Strings.CommunityFix, Tag = typeof(AcceleratorPage) });
        //Tabs.Items.Add(new TabStripItem { Content = Strings.ScriptConfig, Tag = typeof(ScriptPage) });

        Tabs.SelectionChanged += Tabs_SelectionChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (Tabs.SelectedIndex < 0 && Tabs.ItemCount > 0)
            Tabs.SelectedIndex = 0;
        Tabs_SelectionChanged(null, null);
    }

    void ShowServiceWaiting(bool waiting)
    {
        WaitingForService.IsVisible = true;
        ServiceProgress.IsVisible = waiting;
        ServiceLoadingText.IsVisible = waiting;
        ServiceErrorText.IsVisible = !waiting;
        RetryServiceButton.IsVisible = !waiting;
    }

    private async void RetryServiceButton_Click(object? sender, RoutedEventArgs e)
    {
        waitingGeneration++;
        RetryServiceButton.IsEnabled = false;
        ShowServiceWaiting(true);
        try
        {
            await Plugin.Instance.OnInitializeAsync();
            Tabs_SelectionChanged(null, null);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(MainFramePage), ex, "Accelerator service restart failed.");
            ShowServiceWaiting(false);
        }
        finally
        {
            RetryServiceButton.IsEnabled = true;
        }
    }

    private async void Tabs_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs? e)
    {
        if (Tabs.SelectedItem is TabStripItem tab && tab.Tag is Type t)
        {
            if (!Design.IsDesignMode)
            {
                var generation = ++waitingGeneration;
                var servicesReady = Plugin.Instance.ServicesReady;
                ShowServiceWaiting(true);
                try
                {
                    if (await Task.WhenAny(servicesReady, Task.Delay(TimeSpan.FromSeconds(15))) != servicesReady)
                    {
                        if (!IsLoaded || Tabs.SelectedItem != tab || generation != waitingGeneration)
                            return;
                        ShowServiceWaiting(false);
                    }
                    await servicesReady;
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(MainFramePage), ex, "Accelerator services could not be initialized.");
                    if (IsLoaded && Tabs.SelectedItem == tab && generation == waitingGeneration)
                        ShowServiceWaiting(false);
                    return;
                }

                if (!IsLoaded || Tabs.SelectedItem != tab)
                    return;
            }

            WaitingForService.IsVisible = false;
            if (InnerNavFrame.CurrentSourcePageType == t)
                return;
            InnerNavFrame.Navigate(t, null, new SlideNavigationTransitionInfo
            {
                Effect = FrameEffect.GetEffect(lastIndex, Tabs.SelectedIndex),
                FromHorizontalOffset = 70,
            });

            lastIndex = Tabs.SelectedIndex;
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : UserControl
{
    CancellationTokenSource? serviceWait;

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(AcceleratorPage2))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ScriptPage))]
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
        ShowSelectedPage();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        serviceWait?.Cancel();
        serviceWait = null;
        base.OnUnloaded(e);
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
        RetryServiceButton.IsEnabled = false;
        ShowServiceWaiting(true);
        try
        {
            await Plugin.Instance.OnInitializeAsync();
            serviceWait?.Cancel();
            serviceWait = null;
            ShowSelectedPage();
        }
        catch (Exception ex)
        {
            Log.Error(nameof(MainFramePage), ex, "Accelerator service restart failed.");
            if (IsLoaded && !Plugin.Instance.ServicesReady.IsCompletedSuccessfully)
                ShowServiceWaiting(false);
        }
        finally
        {
            RetryServiceButton.IsEnabled = true;
        }
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e) => ShowSelectedPage();

    private async void ShowSelectedPage()
    {
        if (!IsLoaded || serviceWait != null)
            return;

        if (!Design.IsDesignMode && Plugin.Instance.ServicesReady is { IsCompletedSuccessfully: false } ready)
        {
            using var wait = new CancellationTokenSource();
            serviceWait = wait;
            ShowServiceWaiting(true);
            try
            {
                try
                {
                    await ready.WaitAsync(TimeSpan.FromSeconds(15), wait.Token);
                }
                catch (TimeoutException) when (!ready.IsFaulted)
                {
                    wait.Token.ThrowIfCancellationRequested();
                    ShowServiceWaiting(false);
                    await ready.WaitAsync(wait.Token);
                }

                wait.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (wait.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                Log.Error(nameof(MainFramePage), ex, "Accelerator services could not be initialized.");
                if (!wait.IsCancellationRequested)
                    ShowServiceWaiting(false);
                return;
            }
            finally
            {
                if (ReferenceEquals(serviceWait, wait))
                    serviceWait = null;
            }
        }

        WaitingForService.IsVisible = false;
        if (Tabs.SelectedItem is TabStripItem { Tag: Type pageType } && InnerNavFrame.CurrentSourcePageType != pageType)
            InnerNavFrame.Navigate(pageType, null, FrameEffect.NoTransition);
    }
}

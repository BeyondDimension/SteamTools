using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class MainFramePage : UserControl
{
    CancellationTokenSource? serviceWait;

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
        ServiceFailureActions.IsVisible = !waiting;
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

    private async void ViewServiceLogsButton_Click(object? sender, RoutedEventArgs e) => await LogFilesDialog.ShowAsync(this,
        Path.Combine(IApplication.LogDirPath, AssemblyInfo.Accelerator), Strings.CommunityFix_ProxyLog);

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source == Tabs)
            ShowSelectedPage();
    }

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
        var pageType = Tabs.SelectedItem switch
        {
            var item when ReferenceEquals(item, AcceleratorTab) => typeof(AcceleratorPage2),
            var item when ReferenceEquals(item, ScriptTab) => typeof(ScriptPage),
            _ => null,
        };

        if (pageType is not null && InnerNavFrame.CurrentSourcePageType != pageType)
            InnerNavFrame.Navigate(pageType, null, FrameEffect.NoTransition);
    }
}

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace BD.WTTS.UI.Views.Pages;

public partial class CpuSchedulingPage : PageBase<CpuSchedulingPageViewModel>
{
    CpuSchedulingPageViewModel? Scheduler => DataContext as CpuSchedulingPageViewModel;

    public CpuSchedulingPage()
    {
        InitializeComponent();
        DataContext ??= new CpuSchedulingPageViewModel();
#if WINDOWS
        WindowPicker.AddHandler(PointerPressedEvent, PickWindow, RoutingStrategies.Tunnel);
        WindowPicker.PointerReleased += (_, e) => e.Pointer.Capture(null);
        WindowPicker.PointerCaptureLost += (_, _) => WindowPicker.Opacity = 1;
#endif
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Scheduler?.Start(() => IsEffectivelyVisible &&
            TopLevel.GetTopLevel(this) is not Window { WindowState: WindowState.Minimized });
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        Scheduler?.Stop();
        base.OnUnloaded(e);
    }

#if WINDOWS
    void PickWindow(object? sender, PointerPressedEventArgs e)
    {
        if (Scheduler is not { } scheduler || !e.GetCurrentPoint(WindowPicker).Properties.IsLeftButtonPressed)
            return;
        e.Handled = true;
        e.Pointer.Capture(WindowPicker);
        WindowPicker.Opacity = 0;
        try
        {
            IPlatformService.Instance.GetMoveMouseDownWindow(window =>
            {
                int? pid = window?.Process?.Id;
                window?.Process?.Dispose();
                Dispatcher.UIThread.Post(async () =>
                {
                    WindowPicker.Opacity = 1;
                    if (pid.HasValue)
                        await scheduler.SelectProcessAsync(pid.Value);
                });
            });
        }
        catch (Exception ex)
        {
            e.Pointer.Capture(null);
            WindowPicker.Opacity = 1;
            ex.LogAndShowT(nameof(CpuSchedulingPage));
        }
    }
#endif

    void PresetSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedItem: CpuChoice choice } presets)
        {
            Scheduler?.UsePreset(choice);
            presets.SelectedItem = null;
        }
    }

    void ProcessSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.Source == sender && RealtimeConfirm is not null)
            RealtimeConfirm.IsChecked = false;
    }

    void PrioritySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (RealtimeConfirm is not null)
            RealtimeConfirm.IsChecked = false;
    }

    async void Apply_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler)
            await scheduler.ApplyAsync(false, RealtimeConfirm.IsChecked == true);
    }

    async void Restore_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler)
            await scheduler.ApplyAsync(true, false);
    }

    void Reload_Click(object? sender, RoutedEventArgs e) => Scheduler?.ReloadEditor();

    bool MatchPath => MatchFullPath.IsEnabled && MatchFullPath.IsChecked == true;

    async void SaveRule_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler)
            await scheduler.SaveRuleAsync(MatchPath, null);
    }

    async void GameRule_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler)
            await scheduler.SaveRuleAsync(MatchPath, true);
    }

    async void BackgroundRule_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler)
            await scheduler.SaveRuleAsync(MatchPath, false);
    }

    async void DisableAll_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler && sender is CheckBox toggle)
            await scheduler.SetRulesDisabledAsync(toggle.IsChecked == true);
    }

    async void ToggleRule_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler && sender is Button { DataContext: CpuRuleRow row })
            await scheduler.ChangeRuleAsync(row, false);
    }

    async void RemoveRule_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler && sender is Button { DataContext: CpuRuleRow row })
            await scheduler.ChangeRuleAsync(row, true);
    }
}

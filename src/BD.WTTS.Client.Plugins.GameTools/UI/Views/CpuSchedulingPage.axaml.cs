using Avalonia.Controls;
using Avalonia.Interactivity;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class CpuSchedulingPage : PageBase<CpuSchedulingPageViewModel>
{
    TaskDialogEx? processPickerDialog;

    CpuSchedulingPageViewModel? Scheduler => DataContext as CpuSchedulingPageViewModel;

    public CpuSchedulingPage()
    {
        InitializeComponent();
        DataContext ??= new CpuSchedulingPageViewModel();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (!Design.IsDesignMode)
            Scheduler?.Start(() => IsEffectivelyVisible &&
                TopLevel.GetTopLevel(this) is not Window { WindowState: WindowState.Minimized });
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        processPickerDialog?.Hide(TaskDialogStandardResult.Cancel);
        if (!Design.IsDesignMode)
            Scheduler?.Stop();
        base.OnUnloaded(e);
    }

    void PresetSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { SelectedItem: CpuChoice choice } presets)
        {
            Scheduler?.UsePreset(choice);
            presets.SelectedItem = null;
        }
    }

    async void ChooseProcess_Click(object? sender, RoutedEventArgs e)
    {
        if (processPickerDialog is not null || Scheduler is not { } scheduler ||
            TopLevel.GetTopLevel(this) is not { } owner)
            return;
        try
        {
            Classes.Add("pickingProcess");
            scheduler.SetProcessPickerOpen(true);
            var picker = new CpuProcessPicker();
            processPickerDialog = picker.CreateDialog(owner, scheduler);
            if (Equals(await processPickerDialog.ShowAsync(true), TaskDialogStandardResult.OK) &&
                TopLevel.GetTopLevel(this) == owner && ReferenceEquals(Scheduler, scheduler) &&
                picker.SelectedProcess is { } selected)
                scheduler.SelectProcess(selected);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(nameof(CpuSchedulingPage));
        }
        finally
        {
            scheduler.SetProcessPickerOpen(false);
            Classes.Remove("pickingProcess");
            processPickerDialog = null;
            if (TopLevel.GetTopLevel(this) == owner && ChooseProcessButton.IsEffectivelyVisible)
                ChooseProcessButton.Focus();
        }
    }

    async void Apply_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler)
            await scheduler.ApplyAsync(false);
    }

    async void Restore_Click(object? sender, RoutedEventArgs e)
    {
        if (Scheduler is { } scheduler)
            await scheduler.ApplyAsync(true);
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

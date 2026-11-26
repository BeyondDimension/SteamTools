using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace BD.WTTS.UI.Views.Controls;

public partial class CpuProcessPicker : UserControl
{
    public CpuProcessPicker()
    {
        InitializeComponent();
    }

    internal CpuProcessRow? SelectedProcess => ProcessList.SelectedItem as CpuProcessRow;

    internal TaskDialogEx CreateDialog(TopLevel owner, CpuSchedulingPageViewModel scheduler)
    {
        Width = Math.Clamp(owner.Bounds.Width - 96, 240, 560);
        Height = Math.Clamp(owner.Bounds.Height - 160, 140, 420);
        DataContext = scheduler;
        if (scheduler.SelectedProcess is { } current && scheduler.Processes.Contains(current))
            ProcessList.SelectedItem = current;

        var confirm = new TaskDialogButton(Strings.GameTools_Cpu_ChooseProcess, TaskDialogStandardResult.OK)
        {
            IsEnabled = SelectedProcess is not null,
        };
        var dialog = new TaskDialogEx
        {
            Title = Strings.GameTools_Cpu_ChooseProcess,
            Header = Strings.GameTools_Cpu_ChooseProcess,
            Content = this,
            XamlRoot = owner,
        };
        dialog.Classes.Add("disableScroll");
        dialog.Buttons.Add(confirm);
        dialog.Buttons.Add(new TaskDialogButton(Strings.Cancel, TaskDialogStandardResult.Cancel));
        dialog.Opened += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            var confirmButton = dialog.GetVisualDescendants().OfType<Button>()
                .FirstOrDefault(button => ReferenceEquals(button.DataContext, confirm));
            confirmButton?.Classes.Add("accent");
            if (SelectedProcess is { } selected)
                ProcessList.ScrollIntoView(selected);
            ProcessSearch.Focus();
        }, DispatcherPriority.Input);
        ProcessList.SelectionChanged += (_, _) => confirm.IsEnabled = SelectedProcess is not null;
        ProcessList.DoubleTapped += (_, e) =>
        {
            if (SelectedProcess is not null && !IsButtonInput(e.Source) && e.Source is Visual visual &&
                visual.FindAncestorOfType<ListBoxItem>(includeSelf: true) is not null)
                dialog.Hide(TaskDialogStandardResult.OK);
        };
        // The dialog's built-in default handles Enter before focused links can use it.
        dialog.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && SelectedProcess is not null && !IsButtonInput(e.Source))
            {
                e.Handled = true;
                dialog.Hide(TaskDialogStandardResult.OK);
            }
        };
        return dialog;
    }

    static bool IsButtonInput(object? source) =>
        source is Visual visual && visual.FindAncestorOfType<Button>(includeSelf: true) is not null;

    void Search_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Down || ProcessList.ItemCount == 0)
            return;
        if (ProcessList.SelectedIndex < 0)
            ProcessList.SelectedIndex = 0;
        int index = ProcessList.SelectedIndex;
        ProcessList.ScrollIntoView(index);
        (ProcessList.ContainerFromIndex(index) ?? ProcessList).Focus();
        e.Handled = true;
    }
}

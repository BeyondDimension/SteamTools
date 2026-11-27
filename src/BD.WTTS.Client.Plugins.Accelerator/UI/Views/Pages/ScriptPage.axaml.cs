using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Controls;
using FluentAvalonia.UI.Controls;
using System.ComponentModel;

namespace BD.WTTS.UI.Views.Pages;

public partial class ScriptPage : PageBase<ScriptPageViewModel>
{
    public ScriptPage()
    {
        InitializeComponent();
        this.SetViewModel<ScriptPageViewModel>();
        AttachDialogHost();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        AttachDialogHost();
    }

    void AttachDialogHost()
    {
        if (ViewModel != null)
        {
            ViewModel.ShowInspectDialogAsync = ShowInspectDialogCoreAsync;
            ViewModel.ShowScriptStoreDialogAsync = ShowScriptStoreDialogCoreAsync;
        }
    }

    void OpenSettingsPane_Click(object? sender, RoutedEventArgs e)
    {
        IsPaneOpen = !IsPaneOpen;
    }

    async Task<bool> ShowInspectDialogCoreAsync(ScriptAddAndInspectViewModel vm)
    {
        var root = TopLevel.GetTopLevel(this) ?? (IApplication.Instance is App app ? app.MainWindow : null);
        if (root == null)
            return false;

        var contentHeight = root.Bounds.Height > 0 ? Math.Clamp(root.Bounds.Height - 210, 340, 490) : 460;

        var view = new ScriptAddAndInspectView
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = contentHeight,
        };

        var dialog = new TaskDialogEx
        {
            Title = vm.DialogTitle,
            Header = vm.DialogTitle,
            Content = view,
            XamlRoot = root,
            FooterVisibility = TaskDialogFooterVisibility.Never,
        };
        dialog.Classes.Add("disableScroll");
        dialog.Classes.Add("wide");

        TaskDialogButton? confirmButton = null;
        if (vm.IsAddMode)
        {
            confirmButton = new TaskDialogButton(vm.ConfirmButtonText, TaskDialogStandardResult.OK)
            {
                IsEnabled = vm.CanConfirmInstall,
            };
            dialog.Buttons.Add(confirmButton);
            dialog.Buttons.Add(new TaskDialogButton(Strings.Cancel, TaskDialogStandardResult.Cancel));
        }
        else
        {
            dialog.Buttons.Add(new TaskDialogButton(Strings.Close, TaskDialogStandardResult.Close));
        }

        var openScriptStoreRequested = false;
        void OnRequestOpenScriptStore()
        {
            openScriptStoreRequested = true;
            dialog.Hide(TaskDialogStandardResult.Cancel);
        }

        void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (confirmButton == null)
                return;

            if (args.PropertyName is nameof(ScriptAddAndInspectViewModel.CanConfirmInstall)
                or nameof(ScriptAddAndInspectViewModel.ConfirmButtonText)
                or null)
            {
                confirmButton.IsEnabled = vm.CanConfirmInstall;
                confirmButton.Text = vm.ConfirmButtonText;
            }
        }

        object? result = null;
        vm.PropertyChanged += OnVmPropertyChanged;
        vm.OpenScriptStoreRequested += OnRequestOpenScriptStore;
        try
        {
            result = await dialog.ShowAsync(true);
        }
        catch (NullReferenceException) when (result != null)
        {
        }
        finally
        {
            vm.OpenScriptStoreRequested -= OnRequestOpenScriptStore;
            vm.PropertyChanged -= OnVmPropertyChanged;
        }

        if (openScriptStoreRequested && ViewModel != null)
        {
            await ViewModel.OpenScriptStoreFromDialogAsync();
            return false;
        }

        if (vm.IsAddMode && result is TaskDialogStandardResult.OK)
        {
            return await vm.ConfirmInstallAsync();
        }

        return false;
    }

    async Task ShowScriptStoreDialogCoreAsync(ScriptStorePageViewModel vm)
    {
        var root = TopLevel.GetTopLevel(this) ?? (IApplication.Instance is App app ? app.MainWindow : null);
        if (root == null)
            return;

        var contentHeight = root.Bounds.Height > 0 ? Math.Clamp(root.Bounds.Height - 200, 360, 500) : 480;

        var view = new ScriptStorePage
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Height = contentHeight,
        };

        var dialog = new TaskDialogEx
        {
            Title = Strings.ScriptStore,
            Header = Strings.ScriptStore,
            Content = view,
            XamlRoot = root,
            FooterVisibility = TaskDialogFooterVisibility.Never,
        };
        dialog.Classes.Add("disableScroll");
        dialog.Classes.Add("wide");
        dialog.Buttons.Add(new TaskDialogButton(Strings.Close, TaskDialogStandardResult.Close));

        object? result = null;
        try
        {
            result = await dialog.ShowAsync(true);
        }
        catch (NullReferenceException) when (result != null)
        {
        }
    }
}

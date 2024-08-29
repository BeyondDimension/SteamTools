using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Controls;

public partial class AcceleratorPathAskBox : ReactiveUserControl<MessageBoxWindowViewModel>
{
    public AcceleratorPathAskBox()
    {
        InitializeComponent();
    }

    private async void SelectWattAcceleratorInstallPath(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var toplevel = TopLevel.GetTopLevel(this);
        if (toplevel == null)
            return;

        var folders = await toplevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "选择文件夹",
            AllowMultiple = false,
        });

        var folder = folders.FirstOrDefault();
        var path = folder?.Path is { IsAbsoluteUri: true } abs ? abs.LocalPath : folder?.Path?.ToString();

        if (Directory.Exists(path))
        {
            GameAcceleratorSettings.WattAcceleratorDirPath.Value = Path.Combine(path, "WattAccelerator");
        }
    }

    private void OKButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.ViewModel?.Close?.Invoke(true);
    }
}

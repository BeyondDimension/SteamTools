using Avalonia.Controls;
using Avalonia.Interactivity;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class Settings_General : UserControl
{
    bool measuringStorage;

    public Settings_General()
    {
        InitializeComponent();
        StorageSpace.PropertyChanged += StorageSpace_PropertyChanged;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _ = RefreshStorageAsync();
    }

    void StorageSpace_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (IsLoaded && e.Property == FASettingsExpander.IsExpandedProperty && e.GetNewValue<bool>())
            _ = RefreshStorageAsync();
    }

    async Task RefreshStorageAsync()
    {
        if (!StorageSpace.IsExpanded || measuringStorage) return;
        measuringStorage = true;
        try
        {
            await Task.WhenAll(UpdateSizeAsync("App", AppSize), UpdateSizeAsync(IOPath.DirName_AppData, AppDataSize),
                UpdateSizeAsync(IOPath.DirName_Cache, CacheSize), UpdateSizeAsync("Logs", LogSize));
        }
        finally
        {
            measuringStorage = false;
        }

        static async Task UpdateSizeAsync(string tag, TextBlock label) =>
            label.Text = await SettingsPageViewModel.GetStorageSizeAsync(tag);
    }

    async void ViewLogs_Click(object? sender, RoutedEventArgs e) => await LogFilesDialog.ShowAsync(this);
}

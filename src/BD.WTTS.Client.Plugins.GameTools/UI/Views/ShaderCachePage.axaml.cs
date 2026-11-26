using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using BD.WTTS.Services;

namespace BD.WTTS.UI.Views.Pages;

public partial class ShaderCachePage : PageBase<ShaderCachePageViewModel>
{
    public ShaderCachePage()
    {
        InitializeComponent();
        DataContext ??= new ShaderCachePageViewModel();
    }

    public static bool IsSupported => ShaderCacheLocations.IsSupported;

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (!Design.IsDesignMode && ViewModel is { } viewModel)
            await viewModel.RescanAsync();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ViewModel?.CancelScan();
        base.OnDetachedFromVisualTree(e);
    }

    async void Rescan_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.RescanAsync();
    }

    async void Clean_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.CleanAsync(ConfirmCleanupAsync);
    }

    void CancelScan_Click(object? sender, RoutedEventArgs e) => ViewModel?.CancelScan();

    void SelectAll_Click(object? sender, RoutedEventArgs e) => ViewModel?.SelectAll();

    void SelectOld_Click(object? sender, RoutedEventArgs e) => ViewModel?.SelectOldSteamCaches();

    void SelectLarge_Click(object? sender, RoutedEventArgs e) => ViewModel?.SelectLargeCaches();

    async Task<bool> ConfirmCleanupAsync(string message)
    {
        if (TopLevel.GetTopLevel(this) is not { } owner)
            return false;

        var dialog = new ContentDialog
        {
            Title = Strings.GameTools_Shader_Title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = Strings.GameTools_Shader_Clean,
            CloseButtonText = Strings.Cancel,
            DefaultButton = ContentDialogButton.Close,
        };
        return await dialog.ShowAsync(owner) == ContentDialogResult.Primary && TopLevel.GetTopLevel(this) == owner;
    }
}

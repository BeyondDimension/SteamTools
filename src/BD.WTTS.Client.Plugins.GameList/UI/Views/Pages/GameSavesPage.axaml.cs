using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BD.WTTS.GameList.Saves;
using BD.WTTS.Services;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class GameSavesPage : PageBase<GameSavesPageViewModel>
{
    CancellationTokenSource? attachment;

    public GameSavesPage()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
            DataContext ??= new GameSavesPageViewModel();
    }

    protected override async void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Design.IsDesignMode || ViewModel is not { } viewModel)
            return;
        attachment = new();
        await viewModel.EnsureScannedAsync();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        attachment?.Cancel();
        attachment?.Dispose();
        attachment = null;
        ViewModel?.Detach();
        base.OnDetachedFromVisualTree(e);
    }

    async void Rescan_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.RescanAsync(refreshAccounts: true);
    }

    async void RefreshAccounts_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.RefreshAccountsAsync();
    }

    void CancelScan_Click(object? sender, RoutedEventArgs e) => ViewModel?.CancelScan();

    void StopBackground_Click(object? sender, RoutedEventArgs e) => ViewModel?.StopBackgroundScan();

    void SelectAll_Click(object? sender, RoutedEventArgs e) => ViewModel?.SetAllSelected(true);

    void ClearSelection_Click(object? sender, RoutedEventArgs e) => ViewModel?.SetAllSelected(false);

    void ExpandAll_Click(object? sender, RoutedEventArgs e) => ViewModel?.SetAllExpanded(true);

    void CollapseAll_Click(object? sender, RoutedEventArgs e) => ViewModel?.SetAllExpanded(false);

    async void ChangeBackupFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { CanMutate: true } viewModel)
            return;
        if (await PickFolderAsync(Strings.GameList_Saves_ChangeBackupFolder) is { } path)
            await viewModel.ChangeBackupRootAsync(path);
    }

    async void OpenBackupFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await OpenFolderAsync(viewModel.BackupRootPath, create: true);
    }

    async void OpenSteamScreenshotsFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel?.SteamScreenshotsPath is { } path)
            await OpenFolderAsync(path);
    }

    async void BatchBackup_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel)
            await viewModel.BackupSelectedAsync();
    }

    async void BatchCopy_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { CanBatch: true } viewModel)
            return;
        if (await PickFolderAsync(Strings.GameList_Saves_BatchCopy) is { } path)
            await viewModel.BackupSelectedAsync(path);
    }

    async void QuickBackup_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { } viewModel && sender is Button { DataContext: GameSaveGameItemViewModel game })
            await viewModel.BackupGameAsync(game);
    }

    async void BackupElsewhere_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { CanMutate: true } viewModel || sender is not StyledElement { DataContext: GameSaveGameItemViewModel game })
            return;
        if (await PickFolderAsync(Strings.GameList_Saves_BackupElsewhere) is { } path)
            await viewModel.BackupGameAsync(game, path);
    }

    async void OpenGameFolder_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: GameSaveGameItemViewModel game } button)
            return;

        var folders = game.Entry.Locations.Where(GameSaveLocationItemViewModel.CanOpenLocation)
            .Select(location => location.ResolvedDirectory!)
            .Distinct(GameSaveFileSystem.PathComparer)
            .Select(path => (Path: path, Label: string.Format(Strings.GameList_Saves_SaveFolderMenuItem, path))).ToList();
        if (game.ScreenshotPath is { } screenshotPath &&
            !folders.Any(folder => GameSaveFileSystem.PathComparer.Equals(folder.Path, screenshotPath)))
            folders.Add((screenshotPath, string.Format(Strings.GameList_Saves_ScreenshotFolderMenuItem, screenshotPath)));

        if (folders.Count == 1)
            await OpenFolderAsync(folders[0].Path);
        else if (folders.Count > 1)
        {
            var flyout = new MenuFlyout { Placement = PlacementMode.BottomEdgeAlignedRight };
            foreach (var folder in folders)
            {
                var menuItem = new MenuItem { Header = folder.Label };
                menuItem.Click += async (_, _) => await OpenFolderAsync(folder.Path);
                flyout.Items.Add(menuItem);
            }
            flyout.ShowAt(button);
        }
    }

    async void OpenScreenshotFolder_Click(object? sender, RoutedEventArgs e)
    {
        var path = (sender as StyledElement)?.DataContext switch
        {
            GameSaveGameItemViewModel game => game.ScreenshotPath,
            GameSaveScreenshotItemViewModel screenshots => screenshots.Path,
            _ => null,
        };
        if (path != null)
            await OpenFolderAsync(path);
    }

    void TreeRowAction_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: GameSaveTreeRowViewModel row })
            row.Action?.Invoke();
    }

    async void OpenScreenshot_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: GameSaveTreeRowViewModel { ShowImage: true, FullPath: { } path } } ||
            TopLevel.GetTopLevel(this) is not { } owner)
            return;

        try
        {
            if (!await Task.Run(() => GameSaveFileSystem.CheckPath(path, directory: false, allowMissing: true)))
            {
                NotifyError(Strings.GameList_Saves_OpenScreenshotFailed);
                return;
            }
            using var file = await owner.StorageProvider.TryGetFileFromPathAsync(path);
            if (file == null || !await owner.Launcher.LaunchFileAsync(file))
                IPlatformService.Instance.OpenFolderSelectFilePath(new(path));
        }
        catch (Exception ex)
        {
            NotifyError(Strings.GameList_Saves_OpenScreenshotFailed, ex);
        }
    }

    async void OpenLocation_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: GameSaveLocationItemViewModel { Path: { } path } })
            await OpenFolderAsync(path);
    }

    async void CopyPath_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: GameSaveLocationItemViewModel { Path: { } path } })
            return;

        try
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard)
            {
                NotifyError(Strings.GameList_Saves_CopyPathFailed);
                return;
            }
            await clipboard.SetTextAsync(path);
            Toast.Show(ToastIcon.Success, Strings.GameList_Saves_PathCopied);
        }
        catch (Exception ex)
        {
            NotifyError(Strings.GameList_Saves_CopyPathFailed, ex);
        }
    }

    async void OpenSnapshot_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: GameSaveBackupItemViewModel snapshot })
            await OpenFolderAsync(snapshot.Summary.SnapshotDirectory);
    }

    async void QuickRestore_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is { CanMutate: true } viewModel &&
            sender is Button { DataContext: GameSaveBackupItemViewModel snapshot } && viewModel.IsCurrentSnapshot(snapshot))
            await viewModel.RestoreAsync(snapshot.Game, snapshot.Summary.SnapshotDirectory, fromSnapshot: true, ConfirmRestoreAsync);
    }

    async void RestoreExternal_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { CanMutate: true } viewModel || sender is not StyledElement { DataContext: GameSaveGameItemViewModel game })
            return;
        if (await PickFolderAsync(Strings.GameList_Saves_RestoreExternal) is { } path)
            await viewModel.RestoreAsync(game, path, fromSnapshot: false, ConfirmRestoreAsync);
    }

    async void DeleteSnapshot_Click(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is not { CanMutate: true } viewModel ||
            sender is not Button { DataContext: GameSaveBackupItemViewModel snapshot })
            return;

        var root = viewModel.BackupRootPath;
        var dialog = CreateConfirmation(Strings.GameList_Saves_DeleteTitle, Strings.GameList_Saves_Delete,
            WrappedText(string.Format(Strings.GameList_Saves_DeleteConfirm, snapshot.Summary.GameName, snapshot.Timestamp)));
        if (await ShowConfirmationAsync(dialog) && root == viewModel.BackupRootPath)
            await viewModel.DeleteBackupAsync(snapshot);
    }

    async Task<string?> PickFolderAsync(string title)
    {
        if (TopLevel.GetTopLevel(this) is not { } owner)
            return null;
        var lifetime = attachment?.Token ?? CancellationToken.None;
        try
        {
            var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
            });
            using var folder = folders.FirstOrDefault();
            if (lifetime.IsCancellationRequested)
                return null;
            var path = folder?.TryGetLocalPath();
            if (folder != null && path == null)
                NotifyError(Strings.GameList_Saves_FolderPickerFailed);
            return path;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception ex)
        {
            if (!lifetime.IsCancellationRequested)
                NotifyError(Strings.GameList_Saves_FolderPickerFailed, ex);
            return null;
        }
    }

    async Task<GameSaveRestoreSelection?> ConfirmRestoreAsync(GameSaveRestorePreview preview)
    {
        var content = new StackPanel { Spacing = 10 };
        content.Children.Add(WrappedText(Strings.GameList_Saves_RestoreWarning));
        ComboBox? targetPicker = null;
        if (preview.Manifest is { } manifest)
        {
            content.Children.Add(WrappedText(string.Format(Strings.GameList_Saves_RestoreSnapshotInfo,
                manifest.GameName, GameSavesPageViewModel.FormatDate(manifest.CreatedAtUtc))));
            content.Children.Add(WrappedText(Strings.GameList_Saves_RestoreTargets));
            var targetPaths = new StackPanel { Spacing = 4 };
            foreach (var path in preview.TargetDirectories)
                targetPaths.Children.Add(new PathLink { Path = path });
            content.Children.Add(new ScrollViewer
            {
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                MaxHeight = 180,
                Content = targetPaths,
            });
        }
        else
        {
            content.Children.Add(WrappedText(Strings.GameList_Saves_RestoreSource));
            content.Children.Add(new PathLink { Path = preview.SourceFolder });
            content.Children.Add(WrappedText(Strings.GameList_Saves_RestoreChooseTarget));
            targetPicker = CreatePathPicker(preview.RawTargets.Select(location => location.PathDisplay).ToArray(),
                Strings.GameList_Saves_RestoreChooseTarget);
            content.Children.Add(targetPicker);
        }

        var autoBackup = new CheckBox { IsChecked = preview.AutoBackup, Content = WrappedText(Strings.GameList_Saves_RestoreAutoBackup) };
        content.Children.Add(autoBackup);
        var dialog = CreateConfirmation(Strings.GameList_Saves_RestoreTitle, Strings.GameList_Saves_RestoreConfirm, content);
        if (!await ShowConfirmationAsync(dialog))
            return null;

        var location = targetPicker is { SelectedIndex: >= 0 } ? preview.RawTargets[targetPicker.SelectedIndex] : null;
        return targetPicker != null && location == null ? null : new(autoBackup.IsChecked == true, location);
    }

    static TextBlock WrappedText(string content) => new() { Text = content, TextWrapping = TextWrapping.Wrap };

    static ComboBox CreatePathPicker(string[] paths, string name)
    {
        var picker = new ComboBox
        {
            ItemsSource = paths,
            SelectedIndex = 0,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };
        AutomationProperties.SetName(picker, name);
        return picker;
    }

    static ContentDialog CreateConfirmation(string title, string primaryText, Control content) => new()
    {
        Title = title,
        Content = content,
        PrimaryButtonText = primaryText,
        CloseButtonText = Strings.GameList_Saves_Cancel,
        DefaultButton = ContentDialogButton.Close,
    };

    async Task<bool> ShowConfirmationAsync(ContentDialog dialog)
    {
        if (TopLevel.GetTopLevel(this) is not { } owner)
            return false;
        var lifetime = attachment?.Token ?? CancellationToken.None;
        try
        {
            return await dialog.ShowAsync(owner) == ContentDialogResult.Primary && !lifetime.IsCancellationRequested;
        }
        catch (Exception ex)
        {
            NotifyError(Strings.GameList_Saves_ConfirmationFailed, ex);
            return false;
        }
    }

    static async Task OpenFolderAsync(string path, bool create = false)
    {
        try
        {
            var directory = await Task.Run(() =>
            {
                if (create)
                    GameSaveFileSystem.CreateDirectory(path);
                return GameSaveFileSystem.CheckPath(path, directory: true, allowMissing: true) ? new DirectoryInfo(path) : null;
            });
            if (directory != null)
                IPlatformService.Instance.OpenFolderByDirectoryPath(directory);
            else
                NotifyError(Strings.GameList_Saves_OpenFolderFailed);
        }
        catch (Exception ex)
        {
            NotifyError(Strings.GameList_Saves_OpenFolderFailed, ex);
        }
    }

    static void NotifyError(string message, Exception? ex = null)
    {
        if (ex != null)
            Log.Error(nameof(GameSavesPage), ex, "{0}", message);
        Toast.Show(ToastIcon.Error, message);
    }
}

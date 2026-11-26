using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using BD.Avalonia8.Image2;
using BD.WTTS.GameList.Categories;
using BD.WTTS.UI.Views.Controls;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class CategoriesPage : PageBase<CategoriesPageViewModel>
{
    static readonly Avalonia.Platform.Storage.FilePickerFileType JsonFiles = new("JSON") { Patterns = ["*.json"] };
    CancellationTokenSource? lifetime;
    CancellationTokenSource? previewCancellation;
    CancellationTokenSource? refreshCancellation;
    GameCategory? draft;
    bool loadingControls;
    bool initialized;
    bool busy;

    public CategoriesPage()
    {
        InitializeComponent();
        DataContext ??= new CategoriesPageViewModel();
        CategoryModeInput.ItemsSource = Enum.GetValues<CategoryMode>().Select(mode => new Choice<CategoryMode>(mode,
            CategoryLabels.Get("Mode_" + mode))).ToArray();
        var templates = new MenuFlyout();
        foreach (var template in CategoriesPageViewModel.Templates)
        {
            var item = new MenuItem { Header = template.Name };
            item.Click += (_, _) => BeginDraft(new(template.Name, CategoryMode.Auto, template.Rule));
            templates.Items.Add(item);
        }
        TemplatesButton.Flyout = templates;
        var rowTemplate = new FuncDataTemplate<CategoryPreviewRow>((row, _) =>
        {
            var image = new Image2 { Width = 64, Height = 32, DecodeWidth = 128, Stretch = Stretch.UniformToFill };
            SteamAppImageSource.SetIsHeader(image, true);
            SteamAppImageSource.SetApp(image, row.App);
            var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            text.Children.Add(new TextBlock { Text = row.Name, FontWeight = Avalonia.Media.FontWeight.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis });
            text.Children.Add(new TextBlock { Text = row.Details, Opacity = 0.8, TextWrapping = TextWrapping.Wrap });
            var grid = new Grid { ColumnDefinitions = new("Auto,*"), ColumnSpacing = 8, Margin = new(0, 2) };
            grid.Children.Add(image);
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);
            return grid;
        });
        PreviewGames.ItemTemplate = rowTemplate;
        ConflictGames.ItemTemplate = rowTemplate;
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        lifetime = new();
        var token = lifetime.Token;
        if (!initialized)
        {
            await RunAsync(async vm =>
            {
                await vm.InitializeAsync(token);
                token.ThrowIfCancellationRequested();
                initialized = true;
                RegionInput.Text = vm.Region.ToUpperInvariant();
                UpdateInventory(vm);
                UpdateList(vm, null);
                await UpdateConflictsAsync(vm, token);
            }, Strings.GameList_Cat_LoadFailed, requireInitialized: false);
        }
        QueuePreview();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        previewCancellation?.Cancel();
        refreshCancellation?.Cancel();
        lifetime?.Cancel();
        lifetime?.Dispose();
        lifetime = null;
        base.OnUnloaded(e);
    }

    void UpdateButtons()
    {
        NewCategoryButton.IsEnabled = TemplatesButton.IsEnabled = ImportButton.IsEnabled =
            ExportButton.IsEnabled = RunCategoriesButton.IsEnabled = RefreshMetadataButton.IsEnabled =
            ReloadGamesButton.IsEnabled = RegionButton.IsEnabled = CategoryList.IsEnabled =
            EditorPanel.IsEnabled = initialized && !busy;
        CancelRefreshButton.IsVisible = refreshCancellation != null;
    }

    async Task RunAsync(Func<CategoriesPageViewModel, Task> action, string error, bool requireInitialized = true)
    {
        if (busy || (requireInitialized && !initialized) || ViewModel is not { } vm)
            return;
        busy = true;
        UpdateButtons();
        try
        {
            await action(vm);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Log.Error(nameof(CategoriesPage), ex, "Category operation failed.");
            StatusText.Text = error;
        }
        finally
        {
            busy = false;
            UpdateButtons();
        }
    }

    void UpdateInventory(CategoriesPageViewModel vm)
    {
        FindGameInput.ItemsSource = vm.Games.Select(game => new Choice<uint>(game.Facts.AppId,
            $"{game.Facts.Name} ({game.Facts.AppId})")).ToArray();
        var currencies = vm.Currencies.Length == 0 ? Strings.GameList_Cat_CurrencyUnknown : string.Join(", ", vm.Currencies);
        CurrencyText.Text = string.Format(Strings.GameList_Cat_CurrencyDisplay, vm.Region.ToUpperInvariant(), currencies);
        RebuildRuleTree();
        QueuePreview();
    }

    void UpdateList(CategoriesPageViewModel vm, Guid? id)
    {
        loadingControls = true;
        try
        {
            var categories = vm.Categories.Select(category => new Choice<GameCategory>(category,
                vm.AppliedCount(category.Id) is { } count ? $"{category.Name} · {count:N0}" : category.Name)).ToArray();
            CategoryList.ItemsSource = categories;
            CategoryList.SelectedItem = categories.FirstOrDefault(category => category.Value.Id == id);
            ListEmpty.IsVisible = categories.Length == 0;
        }
        finally
        {
            loadingControls = false;
        }
    }

    void CategoryList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!loadingControls)
            ShowDraft((CategoryList.SelectedItem as Choice<GameCategory>)?.Value);
    }

    void BeginDraft(GameCategory category)
    {
        if (ViewModel is not { } vm || !initialized || busy)
            return;
        UpdateList(vm, null);
        ShowDraft(category);
        CategoryName.Focus();
    }

    void ShowDraft(GameCategory? category)
    {
        loadingControls = true;
        try
        {
            draft = category?.Rules.Kind == CategoryRuleKind.Condition
                ? category with { Rules = CategoryRule.Group(CategoryRuleKind.All, category.Rules) }
                : category;
            EditorScroll.IsVisible = category != null;
            EditorEmpty.IsVisible = category == null;
            if (category == null)
            {
                RuleTree.Children.Clear();
                return;
            }
            CategoryName.Text = category.Name;
            CategoryModeInput.SelectedItem = CategoryModeInput.Items.Cast<Choice<CategoryMode>>().First(item => item.Value == category.Mode);
            IncludedInput.Text = string.Join(", ", category.ManualIncluded);
            ExcludedInput.Text = string.Join(", ", category.ManualExcluded);
            FindGameInput.Text = string.Empty;
            DeleteButton.IsVisible = ViewModel?.Categories.Any(item => item.Id == category.Id) == true;
            UpdateMode(category.Mode);
            RebuildRuleTree();
        }
        finally
        {
            loadingControls = false;
            QueuePreview();
        }
    }

    void UpdateMode(CategoryMode mode)
    {
        ManualPanel.IsVisible = mode != CategoryMode.Auto;
        RulePanel.IsVisible = mode != CategoryMode.Manual;
    }

    GameCategory? ReadDraft(bool requireName)
    {
        if (draft is not { } category || CategoryModeInput.SelectedItem is not Choice<CategoryMode> mode)
            return null;
        var name = CategoryName.Text?.Trim() ?? string.Empty;
        if (requireName && name.Length == 0)
            return null;
        var included = mode.Value == CategoryMode.Auto ? category.ManualIncluded : ReadIds(IncludedInput.Text);
        var excluded = mode.Value == CategoryMode.Auto ? category.ManualExcluded : ReadIds(ExcludedInput.Text);
        if (included == null || excluded == null)
            return null;
        return category with { Name = name, Mode = mode.Value, ManualIncluded = included, ManualExcluded = excluded };
    }

    static uint[]? ReadIds(string? value)
    {
        var ids = new HashSet<uint>();
        foreach (var item in (value ?? string.Empty).Split([',', '，', ' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!uint.TryParse(item, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id == 0)
                return null;
            ids.Add(id);
        }
        return ids.Order().ToArray();
    }

    async void QueuePreview()
    {
        if (loadingControls)
            return;
        previewCancellation?.Cancel();
        if (lifetime is null || !initialized || ViewModel is not { } vm)
            return;
        var category = ReadDraft(requireName: false);
        if (category == null)
        {
            PreviewGames.ItemsSource = null;
            PreviewCount.Text = draft == null ? Strings.GameList_Cat_EmptyEditor : Strings.GameList_Cat_InvalidIds;
            return;
        }
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        previewCancellation = cancellation;
        try
        {
            await Task.Delay(180, cancellation.Token);
            var rows = await vm.PreviewAsync(category, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            PreviewGames.ItemsSource = rows;
            PreviewCount.Text = rows.Length == 0 ? Strings.GameList_Cat_NoMatches :
                string.Format(Strings.GameList_Cat_MatchCount, rows.Length);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Log.Error(nameof(CategoriesPage), ex, "Category preview failed.");
            PreviewCount.Text = Strings.GameList_Cat_OperationFailed;
        }
        finally
        {
            if (ReferenceEquals(previewCancellation, cancellation))
                previewCancellation = null;
        }
    }

    async Task UpdateConflictsAsync(CategoriesPageViewModel vm, CancellationToken token)
    {
        var rows = await vm.ConflictsAsync(token);
        token.ThrowIfCancellationRequested();
        ConflictGames.ItemsSource = rows;
        ConflictCount.Text = rows.Length == 0 ? Strings.GameList_Cat_NoConflicts : string.Format(Strings.GameList_Cat_ConflictCount, rows.Length);
    }

    void NewCategory_Click(object? sender, RoutedEventArgs e) =>
        BeginDraft(new(string.Empty, CategoryMode.Auto, CategoryRule.Group(CategoryRuleKind.All)));

    void DraftText_Changed(object? sender, TextChangedEventArgs e) => QueuePreview();

    void CategoryMode_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (!loadingControls && CategoryModeInput.SelectedItem is Choice<CategoryMode> item)
        {
            UpdateMode(item.Value);
            QueuePreview();
        }
    }

    void IncludeGame_Click(object? sender, RoutedEventArgs e) => AddOverride(IncludedInput, ExcludedInput);
    void ExcludeGame_Click(object? sender, RoutedEventArgs e) => AddOverride(ExcludedInput, IncludedInput);

    void AddOverride(TextBox target, TextBox other)
    {
        if (FindGameInput.SelectedItem is not Choice<uint> game)
            return;
        var ids = ReadIds(target.Text);
        var others = ReadIds(other.Text);
        if (ids == null || others == null)
        {
            StatusText.Text = Strings.GameList_Cat_InvalidIds;
            return;
        }
        target.Text = string.Join(", ", ids.Append(game.Value).Distinct().Order());
        other.Text = string.Join(", ", others.Where(id => id != game.Value));
        FindGameInput.Text = string.Empty;
    }

    async void SaveCategory_Click(object? sender, RoutedEventArgs e)
    {
        if (ReadDraft(requireName: true) is not { } category)
        {
            StatusText.Text = string.IsNullOrWhiteSpace(CategoryName.Text) ? Strings.GameList_Cat_EnterName : Strings.GameList_Cat_InvalidIds;
            return;
        }
        await RunAsync(async vm =>
        {
            await vm.SaveAsync(category);
            UpdateList(vm, category.Id);
            ShowDraft(category);
            await UpdateConflictsAsync(vm, lifetime?.Token ?? CancellationToken.None);
            StatusText.Text = Strings.GameList_Cat_Saved;
        }, Strings.GameList_Cat_OperationFailed);
    }

    async void DeleteCategory_Click(object? sender, RoutedEventArgs e)
    {
        if (draft is not { } category || !await ConfirmAsync(Strings.GameList_Cat_DeleteConfirm, Strings.GameList_Cat_Delete))
            return;
        await RunAsync(async vm =>
        {
            await vm.DeleteAsync(category.Id);
            UpdateList(vm, null);
            ShowDraft(null);
            await UpdateConflictsAsync(vm, lifetime?.Token ?? CancellationToken.None);
            StatusText.Text = Strings.GameList_Cat_Deleted;
        }, Strings.GameList_Cat_OperationFailed);
    }

    async Task<bool> ConfirmAsync(string text, string action)
    {
        var owner = TopLevel.GetTopLevel(this);
        if (owner == null)
            return false;
        var dialog = new ContentDialog { Title = action, Content = text, PrimaryButtonText = action, CloseButtonText = Strings.GameList_Cat_Cancel };
        return await dialog.ShowAsync(owner) == ContentDialogResult.Primary;
    }

    async void RunCategories_Click(object? sender, RoutedEventArgs e) =>
        await RunAsync(async vm =>
        {
            var token = lifetime?.Token ?? CancellationToken.None;
            var count = await vm.ApplyAsync(token);
            UpdateList(vm, draft?.Id);
            await UpdateConflictsAsync(vm, token);
            StatusText.Text = string.Format(Strings.GameList_Cat_RunComplete, count);
        }, Strings.GameList_Cat_OperationFailed);

    async void ReloadGames_Click(object? sender, RoutedEventArgs e) =>
        await RunAsync(async vm =>
        {
            await vm.ReloadGamesAsync(lifetime?.Token ?? CancellationToken.None);
            UpdateInventory(vm);
            await UpdateConflictsAsync(vm, lifetime?.Token ?? CancellationToken.None);
            StatusText.Text = string.Format(Strings.GameList_Cat_LibraryLoaded, vm.Games.Length);
        }, Strings.GameList_Cat_LoadFailed);

    async void ApplyRegion_Click(object? sender, RoutedEventArgs e) =>
        await RunAsync(async vm =>
        {
            var region = RegionInput.Text?.Trim().ToLowerInvariant();
            if (!CategoryMetadataProvider.IsRegion(region))
            {
                StatusText.Text = Strings.GameList_Cat_InvalidRegion;
                return;
            }
            await vm.SetRegionAsync(region!);
            RegionInput.Text = vm.Region.ToUpperInvariant();
            await vm.ReadCacheAsync(lifetime?.Token ?? CancellationToken.None);
            UpdateInventory(vm);
            StatusText.Text = Strings.GameList_Cat_RegionSaved;
        }, Strings.GameList_Cat_OperationFailed);

    async void RefreshMetadata_Click(object? sender, RoutedEventArgs e) =>
        await RunAsync(async vm =>
        {
            if (vm.Games.Length == 0)
            {
                StatusText.Text = Strings.GameList_Cat_EmptyLibrary;
                return;
            }
            var token = lifetime?.Token ?? CancellationToken.None;
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
            refreshCancellation = cancellation;
            UpdateButtons();
            MetadataProgress.Value = 0;
            MetadataProgress.Maximum = vm.Games.Length;
            MetadataProgress.IsVisible = true;
            var progress = new Progress<CategoryRefreshProgress>(value =>
            {
                if (!ReferenceEquals(refreshCancellation, cancellation))
                    return;
                MetadataProgress.Value = value.Done;
                StatusText.Text = string.Format(Strings.GameList_Cat_RefreshProgress, value.Done, value.Total);
            });
            try
            {
                StatusText.Text = string.Format(Strings.GameList_Cat_RefreshProgress, 0, vm.Games.Length);
                var result = await vm.RefreshMetadataAsync(progress, cancellation.Token);
                StatusText.Text = result.Stop switch
                {
                    CategoryRefreshStop.RateLimited => Strings.GameList_Cat_RateLimited,
                    CategoryRefreshStop.Unavailable => Strings.GameList_Cat_RefreshUnavailable,
                    _ => string.Format(Strings.GameList_Cat_RefreshComplete, result.Progress.Incomplete),
                };
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                StatusText.Text = Strings.GameList_Cat_RefreshCanceled;
            }
            finally
            {
                refreshCancellation = null;
                MetadataProgress.IsVisible = false;
                UpdateButtons();
                if (!token.IsCancellationRequested)
                {
                    await vm.ReadCacheAsync(token);
                    UpdateInventory(vm);
                }
            }
        }, Strings.GameList_Cat_OperationFailed);

    void CancelRefresh_Click(object? sender, RoutedEventArgs e) => refreshCancellation?.Cancel();

    async void Import_Click(object? sender, RoutedEventArgs e) =>
        await RunAsync(async vm =>
        {
            if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
                return;
            var files = await storage.OpenFilePickerAsync(new() { Title = Strings.GameList_Cat_Import, FileTypeFilter = [JsonFiles] });
            if (files.Count == 0 || !await ConfirmAsync(Strings.GameList_Cat_ImportConfirm, Strings.GameList_Cat_Import))
                return;
            await using var stream = await files[0].OpenReadAsync();
            await vm.ImportAsync(stream);
            UpdateList(vm, null);
            ShowDraft(null);
            await UpdateConflictsAsync(vm, lifetime?.Token ?? CancellationToken.None);
            StatusText.Text = Strings.GameList_Cat_Imported;
        }, Strings.GameList_Cat_ImportFailed);

    async void Export_Click(object? sender, RoutedEventArgs e) =>
        await RunAsync(async vm =>
        {
            if (await ChooseExportFileAsync("categories.json", Strings.GameList_Cat_Export) is not { } file)
                return;
            await using var stream = await file.OpenWriteAsync();
            if (stream.CanSeek)
                stream.SetLength(0);
            await vm.ExportAsync(stream);
            StatusText.Text = Strings.GameList_Cat_Exported;
        }, Strings.GameList_Cat_OperationFailed);

    async Task<IStorageFile?> ChooseExportFileAsync(string name, string title)
    {
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        return storage == null ? null : await storage.SaveFilePickerAsync(new()
        {
            Title = title, FileTypeChoices = [JsonFiles], DefaultExtension = "json", SuggestedFileName = name, ShowOverwritePrompt = true,
        });
    }

    async void SteamExport_Click(object? sender, RoutedEventArgs e)
    {
        if (ReadDraft(requireName: true) is not { } category)
        {
            StatusText.Text = string.IsNullOrWhiteSpace(CategoryName.Text) ? Strings.GameList_Cat_EnterName : Strings.GameList_Cat_InvalidIds;
            return;
        }
        await RunAsync(async vm =>
        {
            var owner = TopLevel.GetTopLevel(this);
            if (owner == null)
                return;
            var rows = await vm.PreviewAsync(category, lifetime?.Token ?? CancellationToken.None);
            var json = vm.SteamExport(category, rows);
            var preview = new StackPanel { Spacing = 10 };
            preview.Children.Add(new TextBlock { Text = Strings.GameList_Cat_SteamReadOnly, TextWrapping = TextWrapping.Wrap });
            preview.Children.Add(new TextBox { Text = json, IsReadOnly = true, AcceptsReturn = true, MaxHeight = 280 });
            var dialog = new ContentDialog
            {
                Title = Strings.GameList_Cat_SteamPreview,
                Content = preview,
                PrimaryButtonText = Strings.GameList_Cat_CopyJson,
                SecondaryButtonText = Strings.GameList_Cat_Export,
                CloseButtonText = Strings.GameList_Cat_Cancel,
            };
            var result = await dialog.ShowAsync(owner);
            if (result == ContentDialogResult.Primary && owner.Clipboard is { } clipboard)
            {
                await clipboard.SetTextAsync(json);
                StatusText.Text = Strings.GameList_Cat_Copied;
            }
            else if (result == ContentDialogResult.Secondary &&
                await ChooseExportFileAsync("steam-collection.json", Strings.GameList_Cat_Export) is { } file)
            {
                await using var stream = await file.OpenWriteAsync();
                if (stream.CanSeek)
                    stream.SetLength(0);
                await using var output = new StreamWriter(stream);
                await output.WriteAsync(json);
                StatusText.Text = Strings.GameList_Cat_Exported;
            }
        }, Strings.GameList_Cat_OperationFailed);
    }

    sealed record Choice<T>(T Value, string Label)
    {
        public override string ToString() => Label;
    }
}

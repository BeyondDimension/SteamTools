using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptPageViewModel : TabItemViewModel
{
    readonly ProxyService service = ProxyService.Current;
    readonly CancellationTokenSource lifetime = new();
    readonly ReadOnlyObservableCollection<ScriptItemViewModel> allScripts;

    public ScriptPageViewModel()
    {
        Settings = new ScriptSettingsViewModel(OpenScriptStoreAsync).AddTo(this);

        var idle = Observable.CombineLatest(
            this.ObserveValue(nameof(IsCheckingUpdates), static vm => vm.IsCheckingUpdates),
            this.ObserveValue(nameof(IsUpdatingAll), static vm => vm.IsUpdatingAll),
            (checking, updating) => !checking && !updating);
        OpenAddFromUrlDialogCommand = Command(() => ShowInspectorAsync(new()));
        PasteQuickUrlCommand = Command(PasteQuickUrlAsync);
        QuickInspectUrlCommand = Command(QuickInspectUrlAsync);
        AddNewScriptCommand = Command(() => ScriptAddAndInspectViewModel.PickScriptFileAsync(path =>
            ShowInspectorAsync(new(showUrlInputBar: false), vm => vm.InspectFromFileAsync(path, lifetime.Token))));
        CheckAllUpdatesCommand = Command(CheckAllUpdatesAsync, idle);
        UpdateAllScriptsCommand = Command(UpdateAllScriptsAsync, idle);
        RefreshALLScriptCommand = Command(async () =>
        {
            await service.RefreshScript();
            Toast.Show(ToastIcon.Success, Strings.RefreshOK);
        });
        ClearFilterCommand = ReactiveCommand.Create(() =>
        {
            SearchText = string.Empty;
            SelectedFilterIndex = (int)ScriptFilterKind.All;
        }).AddTo(this);
        InspectScriptRulesCommand = ItemCommand(item => ShowInspectorAsync(new(isAddMode: false),
            vm => vm.LoadInstalledScriptAsync(item.Model, lifetime.Token)));
        CheckSingleScriptUpdateCommand = ItemCommand(CheckSingleUpdateAsync);
        UpdateScriptItemCommand = ItemCommand(async item =>
        {
            var updated = await service.UpdateScriptAsync(item.Model, lifetime.Token);
            Toast.Show(updated ? ToastIcon.Success : ToastIcon.Error,
                updated ? Strings.Download_ScriptOk : Strings.Script_UpdateStatus_Failed);
        });
        EditScriptItemCommand = ItemCommand(EditScriptAsync);
        RefreshScriptItemCommand = ItemCommand(RefreshScriptAsync);
        DeleteScriptItemCommand = ItemCommand(DeleteScriptAsync);
        CopyScriptLinkCommand = ItemCommand(item => IApplication.CopyToClipboardAsync(item.CopyableLink));
        OpenScriptHomeCommand = ReactiveCommand.Create<ScriptItemViewModel>(item =>
        {
            if (item.HasSourceLink)
                Browser2.Open(item.SourceLink);
        }).AddTo(this);

        service.ProxyScripts.Connect()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Sort(SortExpressionComparer<ScriptDTO>.Ascending(script => script.Order).ThenBy(script => script.Name))
            .Bind(out var proxyScripts)
            .Subscribe().AddTo(this);
        ProxyScripts = proxyScripts;

        var items = service.ProxyScripts.Connect()
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Transform(script => new ScriptItemViewModel(script, service))
            .DisposeMany()
            .AutoRefreshOnObservable(item => Observable.Merge(
                item.ObserveValue(nameof(item.IsEnabled), static vm => vm.IsEnabled).Skip(1),
                item.ObserveValue(nameof(item.IsUpdate), static vm => vm.IsUpdate).Skip(1)))
            .Publish();

        items.Bind(out allScripts).Subscribe(_ => NotifyCounts()).AddTo(this);
        var filter = Observable.CombineLatest(
            this.ObserveValue(nameof(SearchText), static vm => vm.SearchText),
            this.ObserveValue(nameof(SelectedFilterIndex), static vm => vm.SelectedFilterIndex),
            (query, index) => CreateFilter(query, (ScriptFilterKind)index));
        items.Filter(filter)
            .Sort(SortExpressionComparer<ScriptItemViewModel>.Ascending(item => item.Model.Order).ThenBy(item => item.Name))
            .Bind(out var displayed)
            .Subscribe(_ => NotifyEmptyState()).AddTo(this);
        FilteredScripts = displayed;
        items.Connect().AddTo(this);

        service.ObserveValue(nameof(service.IsScriptConfigDirtyWhileProxyRunning), static proxy => proxy.IsScriptConfigDirtyWhileProxyRunning)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(ShowRestartProxyHint))).AddTo(this);
    }

    ICommand Command(Func<Task> execute, IObservable<bool>? canExecute = null)
    {
        var command = ReactiveCommand.CreateFromTask(execute, canExecute).AddTo(this);
        command.ThrownExceptions.Subscribe(ShowCommandError).AddTo(this);
        return command;
    }

    ICommand ItemCommand(Func<ScriptItemViewModel, Task> execute)
    {
        var command = ReactiveCommand.CreateFromTask<ScriptItemViewModel>(item =>
            item is null || item.IsBusy ? Task.CompletedTask : execute(item)).AddTo(this);
        command.ThrownExceptions.Subscribe(ShowCommandError).AddTo(this);
        return command;
    }

    void ShowCommandError(Exception ex)
    {
        if (!Disposed && ex is not OperationCanceledException)
            ex.LogAndShowT();
    }

    static Func<ScriptItemViewModel, bool> CreateFilter(string query, ScriptFilterKind filter)
    {
        var text = query.Trim();
        const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
        return item => (filter switch
        {
            ScriptFilterKind.Enabled => item.IsEnabled,
            ScriptFilterKind.Disabled => !item.IsEnabled,
            ScriptFilterKind.Updatable => item.IsUpdate,
            _ => true,
        }) && (text.Length == 0 || item.Name.Contains(text, comparison) || item.AuthorName.Contains(text, comparison)
            || item.Describe.Contains(text, comparison)
            || item.Model.MatchDomainNamesArray.Any(rule => rule.Contains(text, comparison)));
    }

    void NotifyCounts()
    {
        this.RaisePropertyChanged(nameof(TotalScriptCount));
        this.RaisePropertyChanged(nameof(EnabledScriptCount));
        this.RaisePropertyChanged(nameof(AvailableUpdateCount));
        this.RaisePropertyChanged(nameof(HasAvailableUpdates));
        this.RaisePropertyChanged(nameof(HasAnyScripts));
        this.RaisePropertyChanged(nameof(TotalCountText));
        this.RaisePropertyChanged(nameof(EnabledCountText));
        this.RaisePropertyChanged(nameof(UpdatesCountText));
        this.RaisePropertyChanged(nameof(UpdateBannerMessage));
        NotifyEmptyState();
    }

    void NotifyEmptyState()
    {
        this.RaisePropertyChanged(nameof(IsFilteredEmpty));
        this.RaisePropertyChanged(nameof(IsCompletelyEmpty));
    }

    async Task PasteQuickUrlAsync()
    {
        var text = (await Clipboard2.GetTextAsync())?.Trim();
        if (!string.IsNullOrEmpty(text))
            QuickUrlText = text;
    }

    async Task QuickInspectUrlAsync()
    {
        var url = QuickUrlText.Trim();
        var added = await ShowInspectorAsync(new(), url.Length == 0 ? null : vm => vm.InspectFromUrlAsync(url, lifetime.Token));
        if (added && QuickUrlText.Trim() == url)
            QuickUrlText = string.Empty;
    }

    async Task<bool> ShowInspectorAsync(ScriptAddAndInspectViewModel inspector,
        Func<ScriptAddAndInspectViewModel, Task>? load = null)
    {
        using (inspector)
        {
            if (ShowInspectDialogAsync is not { } show)
                return false;

            if (load != null)
                _ = load(inspector);
            return await show(inspector);
        }
    }

    internal Task OpenScriptStoreFromDialogAsync() => OpenScriptStoreAsync();

    async Task OpenScriptStoreAsync()
    {
        if (!UserService.Current.IsAuthenticated)
        {
            Toast.Show(ToastIcon.Error, Strings.L10nAccel_146);
            UserService.Current.ShowWindow();
            return;
        }
        using var model = new ScriptStorePageViewModel();
        if (ShowScriptStoreDialogAsync is { } show)
            await show(model);
        else
            await IWindowManager.Instance.ShowTaskDialogAsync(model, Strings.ScriptStore,
                pageContent: new ScriptStorePage(), isOkButton: false);
    }

    async Task CheckAllUpdatesAsync()
    {
        IsCheckingUpdates = true;
        try
        {
            var result = await service.CheckScriptUpdatesAsync(showToast: true, cancellationToken: lifetime.Token);
            LastUpdateCheckSummary = ProxyService.GetScriptUpdateSummary(result);
        }
        finally
        {
            IsCheckingUpdates = false;
        }
    }

    async Task CheckSingleUpdateAsync(ScriptItemViewModel item)
    {
        item.IsCheckingUpdate = true;
        try
        {
            await service.CheckSingleScriptUpdateAsync(item.Model, lifetime.Token);
        }
        finally
        {
            item.IsCheckingUpdate = false;
        }
    }

    async Task UpdateAllScriptsAsync()
    {
        IsUpdatingAll = true;
        try
        {
            var updates = allScripts.Where(item => item.IsUpdate && !item.IsBusy).Select(item => item.Model).ToArray();
            var count = 0;
            foreach (var script in updates)
            {
                if (await service.UpdateScriptAsync(script, lifetime.Token))
                    count++;
            }

            var failures = updates.Length - count;
            LastUpdateCheckSummary = failures == 0 ? Strings.Script_UpdateAllComplete.Format(count)
                : Strings.Script_UpdateAllPartial.Format(count, failures);
            Toast.Show(failures == 0 ? ToastIcon.Success : ToastIcon.Warning, LastUpdateCheckSummary);
        }
        finally
        {
            IsUpdatingAll = false;
        }
    }

    async Task EditScriptAsync(ScriptItemViewModel item)
    {
        var path = Path.Combine(Plugin.Instance.AppDataDirectory, item.Model.FilePath);
        if (!File.Exists(path))
        {
            await DeleteScriptAsync(item, Strings.Script_NoFileDeleteItem);
            return;
        }

        IPlatformService.Instance.OpenFileByTextReader(path);
        if (await MessageBox.ShowAsync(Strings.Script_EditTxt, button: MessageBox.Button.OKCancel) == MessageBox.Result.OK)
            await RefreshScriptAsync(item);
    }

    async Task RefreshScriptAsync(ScriptItemViewModel item)
    {
        var path = Path.Combine(Plugin.Instance.AppDataDirectory, item.Model.FilePath);
        if (!File.Exists(path))
        {
            await DeleteScriptAsync(item, Strings.Script_NoFileDeleteItem);
            return;
        }

        var result = await service.RefreshInstalledScriptAsync(item.Model);
        Toast.Show(result.IsSuccess ? ToastIcon.Success : ToastIcon.Error, result.IsSuccess ? Strings.RefreshOK : result.Message);
    }

    Task DeleteScriptAsync(ScriptItemViewModel item) => DeleteScriptAsync(item, Strings.Script_DeleteItem);

    async Task DeleteScriptAsync(ScriptItemViewModel item, string prompt)
    {
        if (item.IsBasics)
        {
            Toast.Show(ToastIcon.Warning, Strings.Script_DeleteError);
            return;
        }
        if (await MessageBox.ShowAsync(prompt, button: MessageBox.Button.OKCancel) != MessageBox.Result.OK)
            return;

        var result = await service.DeleteScriptAsync(item.Model);
        Toast.Show(result.IsSuccess ? ToastIcon.Success : ToastIcon.Error, result.Message);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            ShowInspectDialogAsync = null;
            ShowScriptStoreDialogAsync = null;
            lifetime.Cancel();
            lifetime.Dispose();
        }
        base.Dispose(disposing);
    }
}

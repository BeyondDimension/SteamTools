using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptStorePageViewModel : ViewModelBase
{
    readonly ProxyService service = ProxyService.Current;
    readonly SourceList<ScriptStoreItemViewModel> scriptRows = new();
    readonly CancellationTokenSource lifetime = new();
    string? requestedQuery;
    int requestVersion;

    public ScriptStorePageViewModel()
    {
        var filter = this.ObserveValue(nameof(SelectedFilterIndex), static vm => vm.SelectedFilterIndex)
            .Select(CreateFilter);
        scriptRows.Connect()
            .DisposeMany()
            .AutoRefreshOnObservable(item => Observable.Merge(
                item.ObserveValue(nameof(item.IsInstalled), static row => row.IsInstalled).Skip(1),
                item.ObserveValue(nameof(item.IsUpdate), static row => row.IsUpdate).Skip(1)))
            .Filter(filter)
            .Sort(SortExpressionComparer<ScriptStoreItemViewModel>.Ascending(item => item.Model.Order).ThenBy(item => item.Name))
            .Bind(out var filteredScripts)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(ShowEmptyState))).AddTo(this);
        FilteredScripts = filteredScripts;

        RefreshCommand = ReactiveCommand.CreateFromTask(() => ReloadAsync(force: true)).AddTo(this);
        ClearFilterCommand = ReactiveCommand.Create(() =>
        {
            SearchText = string.Empty;
            SelectedFilterIndex = 0;
        }).AddTo(this);
        DownloadScriptItemCommand = ReactiveCommand.CreateFromTask<ScriptStoreItemViewModel>(DownloadScriptItemAsync).AddTo(this);

        this.ObserveValue(nameof(SearchText), static vm => vm.SearchText)
            .Skip(1)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(value => { _ = ReloadAsync(); }).AddTo(this);
        service.ProxyScripts.Connect()
            .AutoRefreshOnObservable(script => script.ObserveValue(nameof(script.Version), static model => model.Version).Skip(1))
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(_ => SynchronizeInstalledState()).AddTo(this);
        scriptRows.AddTo(this);

        _ = ReloadAsync();
    }

    static Func<ScriptStoreItemViewModel, bool> CreateFilter(int index) => index switch
    {
        1 => item => !item.IsInstalled,
        2 => item => item.IsInstalled,
        3 => item => item.IsUpdate,
        _ => _ => true,
    };

    async Task ReloadAsync(bool force = false)
    {
        if (Disposed)
            return;

        var query = SearchText.Trim();
        if (!force && query == requestedQuery)
            return;

        requestedQuery = query;
        var version = ++requestVersion;
        var token = lifetime.Token;
        ErrorMessage = null;
        IsLoading = true;
        try
        {
            var response = await IMicroServiceClient.Instance.Script.Query(name: query, pageSize: 100,
                errorAppendText: Strings.ScriptShop_GetTableError).WaitAsync(token);
            if (Disposed || version != requestVersion)
                return;

            if (!response.IsSuccess || response.Content is null)
            {
                ReportLoadError(response.Message);
                return;
            }

            var installed = GetInstalledScripts();
            var rows = response.Content.DataSource.Select(script =>
            {
                var row = new ScriptStoreItemViewModel(script);
                row.SynchronizeInstalledState(FindInstalledScript(installed, script));
                return row;
            }).ToArray();

            scriptRows.Edit(items =>
            {
                items.Clear();
                items.AddRange(rows);
            });
            this.RaisePropertyChanged(nameof(CountSummary));
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            if (!Disposed && version == requestVersion)
            {
                Log.Error(nameof(ScriptStorePageViewModel), ex, "Unable to load the script Workshop.");
                ReportLoadError(null);
            }
        }
        finally
        {
            if (!Disposed && version == requestVersion)
                IsLoading = false;
        }
    }

    void ReportLoadError(string? message)
    {
        ErrorMessage = string.IsNullOrWhiteSpace(message) ? Strings.Script_Store_LoadFailed : message;
        scriptRows.Clear();
        this.RaisePropertyChanged(nameof(CountSummary));
    }

    ILookup<Guid, ScriptDTO> GetInstalledScripts()
        => service.ProxyScripts.Items.Where(script => script.Id.HasValue).ToLookup(script => script.Id!.Value);

    static ScriptDTO? FindInstalledScript(ILookup<Guid, ScriptDTO> installed, ScriptDTO model)
        => model.Id is { } id ? installed[id].FirstOrDefault() : null;

    void SynchronizeInstalledState()
    {
        var installed = GetInstalledScripts();
        foreach (var item in scriptRows.Items)
            item.SynchronizeInstalledState(FindInstalledScript(installed, item.Model));
        this.RaisePropertyChanged(nameof(CountSummary));
    }

    async Task DownloadScriptItemAsync(ScriptStoreItemViewModel item)
    {
        if (Disposed || item is null || (!item.ShowGetButton && !item.ShowUpdateButton))
            return;

        var token = lifetime.Token;
        item.Model.IsLoading = true;
        try
        {
            var updated = await service.UpdateScriptAsync(item.Model, token);
            if (!Disposed)
                Toast.Show(updated ? ToastIcon.Success : ToastIcon.Error,
                    updated ? Strings.Download_ScriptOk : Strings.Script_UpdateStatus_Failed);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            if (!Disposed)
            {
                Log.Error(nameof(ScriptStorePageViewModel), ex, "Unable to install the Workshop script.");
                Toast.Show(ToastIcon.Error, Strings.Script_UpdateStatus_Failed);
            }
        }
        finally
        {
            item.Model.IsLoading = false;
            if (!Disposed)
                SynchronizeInstalledState();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !Disposed)
        {
            lifetime.Cancel();
            lifetime.Dispose();
        }
        base.Dispose(disposing);
    }
}

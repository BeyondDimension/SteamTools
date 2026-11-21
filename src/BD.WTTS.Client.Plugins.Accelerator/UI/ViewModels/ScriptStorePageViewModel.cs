using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptStorePageViewModel : ViewModelBase
{
    int searchVersion;

    public ScriptStorePageViewModel()
    {
        _ScriptsSourceList = new SourceList<ScriptDTO>();

        _ScriptsSourceList
            .Connect()
            //.Filter(scriptFilter)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Sort(SortExpressionComparer<ScriptDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
            .Bind(out _Scripts)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsScriptsEmpty))).AddTo(this);

        DownloadScriptItemCommand = ReactiveCommand.Create<ScriptDTO>(DownloadScriptItem);

        this.ObserveValue(nameof(SearchText), static x => x.SearchText)
            .Subscribe(query => _ = InitializeScriptListAsync(query)).AddTo(this);
    }

    public void DownloadScriptItem(ScriptDTO model)
    {
        ProxyService.Current.DownloadScript(model);
    }

    async Task InitializeScriptListAsync(string? query)
    {
        var version = ++searchVersion;
        IsLoading = true;
        try
        {
            var response = await IMicroServiceClient.Instance.Script.Query(name: query, pageSize: 100, errorAppendText: Strings.ScriptShop_GetTableError);
            if (Disposed || version != searchVersion) return;

            _ScriptsSourceList.Edit(items =>
            {
                items.Clear();
                if (!response.IsSuccess || response.Content is null) return;

                foreach (var item in response.Content.DataSource)
                {
                    var installed = ProxyService.Current.ProxyScripts.Items.FirstOrDefault(x => x.Id == item.Id);
                    if (installed != null)
                    {
                        item.LocalId = installed.LocalId;
                        item.IsExist = true;
                        if (installed.Version != item.Version)
                        {
                            item.IsUpdate = true;
                            item.NewVersion = item.Version;
                            item.Version = installed.Version;
                        }
                    }
                    items.Add(item);
                }
            });
        }
        catch (Exception ex)
        {
            if (!Disposed && version == searchVersion) ex.LogAndShowT();
        }
        finally
        {
            if (!Disposed && version == searchVersion) IsLoading = false;
        }
    }
}

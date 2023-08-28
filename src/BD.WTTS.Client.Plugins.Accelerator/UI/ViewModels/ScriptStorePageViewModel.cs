using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptStorePageViewModel : ViewModelBase
{
    public ScriptStorePageViewModel()
    {
        _ScriptsSourceList = new SourceList<ScriptDTO>();

        _ScriptsSourceList
            .Connect()
            //.Filter(scriptFilter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Sort(SortExpressionComparer<ScriptDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
            .Bind(out _Scripts)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsScriptsEmpty)));

        DownloadScriptItemCommand = ReactiveCommand.Create<ScriptDTO>(DownloadScriptItem);

        this.WhenAnyValue(x => x.SearchText)
            .Subscribe(x =>
            {
                InitializeScriptList();
            });
    }

    public void DownloadScriptItem(ScriptDTO model)
    {
        ProxyService.Current.DownloadScript(model);
    }

    async void InitializeScriptList()
    {
        var client = IMicroServiceClient.Instance.Script;
        var response = await client.Query(name: SearchText, pageSize: 100, errorAppendText: Strings.ScriptShop_GetTableError);
        if (!response.IsSuccess || response.Content == null)
        {
            return;
        }
        response.Content.DataSource.ForEach(item =>
        {
            var old = ProxyService.Current.ProxyScripts.Items.FirstOrDefault(x => x.Id == item.Id);
            if (old != null)
            {
                item.LocalId = old.LocalId;
                item.IsExist = true;
                if (old.Version != item.Version)
                {
                    item.IsUpdate = true;
                    item.NewVersion = item.Version;
                    item.Version = old.Version;
                }
            }
        });
        _ScriptsSourceList.Clear();
        _ScriptsSourceList.AddRange(response.Content.DataSource);
    }
}

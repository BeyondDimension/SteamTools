using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptPageViewModel : TabItemViewModel
{
    public ScriptPageViewModel()
    {
        //var scriptFilter = this.WhenAnyValue(x => x.SearchText).Select(ScriptFilter);

        ProxyService.Current.ProxyScripts
            .Connect()
            //.Filter(scriptFilter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Sort(SortExpressionComparer<ScriptDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
            .Bind(out _ProxyScripts)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsProxyScriptsEmpty)));
    }
}
